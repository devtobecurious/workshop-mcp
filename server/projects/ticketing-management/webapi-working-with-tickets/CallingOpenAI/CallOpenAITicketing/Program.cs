using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;

using System.Runtime.CompilerServices;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string? model = config["ModelName"];
    string? key = config["OpenAIKey"];

    var openAiClient = new OpenAIClient(key).GetChatClient(model);
    return openAiClient.AsIChatClient();
});

var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "Ticketing",
    Command = "ticketing-mcp",
}));
builder.Services.AddSingleton<McpClient>(mcpClient);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/chat", async (IChatClient chatClient, McpClient mcpClient, ChatRequest request) =>
{
    var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

    var messages = request.ToMessages();

    var streamingUpdates = new List<ChatResponseUpdate>();
    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools.Cast<AITool>()] }))
    {
        streamingUpdates.Add(update);
        if (!string.IsNullOrEmpty(update.Text))
        {
            streamingUpdates.Add(update);
        }
    }

    var assistantContents = streamingUpdates
        .SelectMany(u => u.Contents)
        .ToList();

    var functionCalls = assistantContents
        .OfType<FunctionCallContent>()
        .ToList();

    if (functionCalls.Any())
    {
        messages.Add(new ChatMessage(ChatRole.Assistant, assistantContents));

        foreach (var functionCall in functionCalls)
        {
            var toolResult = await mcpClient.CallToolAsync(functionCall.Name, (IReadOnlyDictionary<string, object?>?)functionCall.Arguments).ConfigureAwait(false);
            messages.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(functionCall.CallId, toolResult)]));
        }

        var finalUpdates = new List<ChatResponseUpdate>();
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools.Cast<AITool>()] }))
        {
            if (update.Text != null)
            {
                finalUpdates.Add(update);
            }
        }
        streamingUpdates.AddRange(finalUpdates);
    }

    var responseText = string.Concat(
        streamingUpdates
            .Where(u => u.Text != null)
            .Select(u => u.Text));

    return Results.Ok(new { response = responseText });
});

app.MapPost("/api/chat/stream", async (IChatClient chatClient, McpClient mcpClient, ChatRequest request, CancellationToken cancellationToken) =>
{
    var streaming = new ChatStreaming().GetAll(chatClient, mcpClient, request, cancellationToken);

    return TypedResults.ServerSentEvents(streaming, eventType: "ticket");
});



app.Run();

public class ChatStreaming
{
    public async IAsyncEnumerable<ChatResponseUpdate> GetAll(IChatClient chatClient, McpClient mcpClient, ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        var messages = request.ToMessages();

        var streamingUpdates = new List<ChatResponseUpdate>();
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools.Cast<AITool>()] }))
        {
            streamingUpdates.Add(update);
            yield return update;
        }

        var assistantContents = streamingUpdates
            .SelectMany(u => u.Contents)
            .ToList();

        var functionCalls = assistantContents
            .OfType<FunctionCallContent>()
            .ToList();

        if (functionCalls.Count != 0)
        {
            messages.Add(new ChatMessage(ChatRole.Assistant, assistantContents));

            foreach (var functionCall in functionCalls)
            {
                var toolResult = await mcpClient.CallToolAsync(functionCall.Name, (IReadOnlyDictionary<string, object?>?)functionCall.Arguments).ConfigureAwait(false);
                messages.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(functionCall.CallId, toolResult)]));
            }

            var finalUpdates = new List<ChatResponseUpdate>();
            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools.Cast<AITool>()] }))
            {
                if (update.Text != null)
                {
                    finalUpdates.Add(update);
                    yield return update;
                }
            }
            streamingUpdates.AddRange(finalUpdates);
        }
    }
}

public record ChatRequest
{
    public string Prompt { get; init; } = string.Empty;
    public string? SystemMessage { get; init; }

    // Méthode helper pour créer la liste de messages
    public List<ChatMessage> ToMessages()
    {
        return new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, SystemMessage ?? "You are a helpful assistant that summarizes support tickets."),
            new ChatMessage(ChatRole.User, Prompt)
        };
    }
}