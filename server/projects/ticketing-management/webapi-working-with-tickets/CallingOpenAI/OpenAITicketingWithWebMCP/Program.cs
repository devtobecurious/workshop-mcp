using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using OpenAI;

using System.Globalization;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
string? model = config["ModelName"];
string? key = config["OpenAIKey"];

var openAiClient = new OpenAIClient(key).GetChatClient(model);
using IChatClient chatClient = openAiClient.AsIChatClient();


await using var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new()
{
    Endpoint = new Uri("http://localhost:5237"),
    Name = "Ticketing",
}));

// List available resources
var resources = await mcpClient.ListResourcesAsync().ConfigureAwait(false);
foreach (var item in resources)
{
    Console.WriteLine("Resource: " + item.Name);
    var result = await item.ReadAsync().ConfigureAwait(false);
}

var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

while (true)
{
    Console.WriteLine("\nPrompt ?");
    var prompts = await mcpClient.ListPromptsAsync().ConfigureAwait(false);

    int i = 0;
    foreach (var existingPrompt in prompts)
    {
        Console.WriteLine($"{i} - {existingPrompt.Name}");
        i++;
    }
    Console.WriteLine($"{i} - Nouveau");
    Console.WriteLine("Lequel ?");
    string prompt = string.Empty;
    string? promptChoice = Console.ReadLine();

    var reglementation = await resources.First().ReadAsync().ConfigureAwait(false);

    var messages = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, "You are a helpful assistant that summarizes support tickets."),
        new ChatMessage(ChatRole.User, reglementation.Contents.OfType<TextResourceContents>().First().Text)
    };

    if (int.Parse(promptChoice) < prompts.Count)
    {
        var selectedPrompt = prompts[int.Parse(promptChoice!, CultureInfo.InvariantCulture)];
        var promptArgs = new Dictionary<string, object?>();
        if (selectedPrompt.ProtocolPrompt.Arguments?.Any() == true)
        {
            Console.WriteLine($"\n=== Arguments pour '{selectedPrompt.Name}' ===");
            foreach (var arg in selectedPrompt.ProtocolPrompt.Arguments)
            {
                Console.Write($"{arg.Name}{(arg.Required.GetValueOrDefault() ? "*" : "")} ({arg.Description}): ");
                string value = Console.ReadLine();

                if (arg.Required.GetValueOrDefault() && string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine($"L'argument '{arg.Name}' est requis!");
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    promptArgs[arg.Name] = value;
                }
            }
        }

        var promptResult = await mcpClient.GetPromptAsync(
            selectedPrompt.Name,
            (IReadOnlyDictionary<string, object?>?)promptArgs
        ).ConfigureAwait(false);

        messages = promptResult.Messages.Select(m => m.ToChatMessage()).ToList();
    }
    else
    {
        Console.WriteLine("Le prompt ? ");
        prompt = Console.ReadLine() ?? string.Empty;
        messages.Add(new ChatMessage(ChatRole.User, prompt));
    }

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