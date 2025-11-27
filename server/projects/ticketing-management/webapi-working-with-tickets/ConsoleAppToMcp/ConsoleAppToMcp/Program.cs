using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "Ticketing",
    Command = "ticketing-mcp",
});

var client = await McpClient.CreateAsync(clientTransport);

foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"{tool.Name} ({tool.Description})");
}

var result = await client.CallToolAsync
(
    "get_tickets",
    new Dictionary<string, object?>(),
    cancellationToken: CancellationToken.None
);

Console.WriteLine(result.Content.OfType<TextContentBlock>().First().Text);