using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ticketing_mcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddMcpServer(options =>
{
})
.WithTools<ReadTicketsTool>()
.WithStdioServerTransport();


await builder.Build().RunAsync();