using System.Net.Http.Headers;

using TicketsServerMCP.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<ReadTicketsTool>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.MapMcp();

app.Run();
