using ModelContextProtocol.Server;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using TicketsMCPResourcePromptsS.Models;
using TicketsMCPResourcePromptsS.Services;

namespace TicketsMCPResourcePromptsS.Tools
{
    [McpServerToolType]
    internal class ReadTicketsTool
    {
        private readonly GetTicketsService getTicketsService;

        public ReadTicketsTool(GetTicketsService getTicketsService)
        {
            this.getTicketsService = getTicketsService;
        }

        #region Public methods
        [McpServerTool, Description("Get all tickets from the ticketing system.")]
        public Task<string> GetTickets()
        {

            return Task.FromResult(string.Join("\n--\n", getTicketsService.GetAll().Select(item => $"* title : {item.Title} \n * description : {item.Description} \n * status : {item.Status}")));
        }
        #endregion
    }
}
