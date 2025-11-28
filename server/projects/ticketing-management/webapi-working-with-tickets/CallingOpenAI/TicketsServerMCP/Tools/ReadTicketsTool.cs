using ModelContextProtocol.Server;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using TicketsServerMCP.Models;

namespace TicketsServerMCP.Tools
{
    [McpServerToolType]
    internal class ReadTicketsTool
    {
        #region Public methods
        [McpServerTool, Description("Get all tickets from the ticketing system.")]
        public static Task<string> GetTickets()
        {

            return Task.FromResult(string.Join("\n--\n", TicketDatabase.GetAllTickets().Select(item => $"* title : {item.Title} \n * description : {item.Description} \n * status : {item.Status}")));
        }
        #endregion
    }
}
