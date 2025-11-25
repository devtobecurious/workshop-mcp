using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ticketing_mcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMcpServer(options =>
{
})
.WithTools<ReadTicketsTool>()
.WithStdioServerTransport();


await builder.Build().RunAsync();