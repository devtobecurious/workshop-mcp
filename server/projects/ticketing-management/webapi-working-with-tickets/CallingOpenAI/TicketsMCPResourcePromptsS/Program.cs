using System.Net.Http.Headers;

using TicketsMCPResourcePromptsS.Prompts;
using TicketsMCPResourcePromptsS.Resources;
using TicketsMCPResourcePromptsS.Services;
using TicketsMCPResourcePromptsS.Tools;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GetTicketsService>()
                .AddMcpServer()
                .WithHttpTransport()
                .WithTools<ReadTicketsTool>()
                .WithTools<AnalyzePriorityTool>()
                .WithPrompts<PromptsContainer>()
                .WithResources<TicketingResource>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.MapMcp();

app.Run();
