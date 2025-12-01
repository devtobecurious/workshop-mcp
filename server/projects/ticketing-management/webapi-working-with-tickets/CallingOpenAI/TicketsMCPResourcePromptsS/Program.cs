using System.Net.Http.Headers;

using TicketsMCPResourcePromptsS.Prompts;
using TicketsMCPResourcePromptsS.Tools;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<ReadTicketsTool>()
                .WithTools<AnalyzePriorityTool>()
                .WithPrompts<PromptsContainer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.MapMcp();

app.Run();
