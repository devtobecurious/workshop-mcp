using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ModelContextProtocol;
using ModelContextProtocol.Client;

using OpenAI;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
string? model = config["ModelName"];
string? key = config["OpenAIKey"];

var openAiClient = new OpenAIClient(key).GetChatClient(model);
using IChatClient chatClient = openAiClient.AsIChatClient();


await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "Ticketing",
    Command = "ticketing-mcp",
}));

var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

while (true)
{
    Console.WriteLine("\nPrompt ?");
    string prompt = Console.ReadLine();
    var messages = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, "You are a helpful assistant that summarizes support tickets."),
        new ChatMessage(ChatRole.User, prompt)
    };

    var streamingUpdates = new List<ChatResponseUpdate>();
    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools.Cast<AITool>()] }))
    {
        streamingUpdates.Add(update);
        if (!string.IsNullOrEmpty(update.Text))
        {
            Console.Write(update.Text);
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

        Console.WriteLine("\n[Traitement de la réponse finale...]");
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools.Cast<AITool>()] }))
        {
            if (update.Text != null)
            {
                Console.Write(update.Text);
            }
        }
    }
}


//ChatResponse response = await client.GetResponseAsync(messages, new ChatOptions { MaxOutputTokens = 600, ToolMode = ChatToolMode.Auto, Tools = [.. tools.Cast<AITool>()] });

//if (response.Messages.Count > 0)
//{
//    Console.WriteLine("Response:");
//    foreach (var message in response.Messages)
//    {
//        foreach (var content in message.Contents)
//        {
//            if(content is FunctionCallContent toolCall)
//            {
//            }
//        }
//    }
//}