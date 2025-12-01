using ModelContextProtocol.Server;

using System.ComponentModel;

using TicketsMCPResourcePromptsS.Models;

namespace TicketsMCPResourcePromptsS.Tools
{
    [McpServerToolType]
    public class AnalyzePriorityTool
    {
        #region Public methods
        [McpServerTool, Description("Analyze ticket priorities from the ticketing system.")]
        public static Task<string> AnalyzePriority()
        {

            var tickets = TicketDatabase.GetAllTickets();
            var prioritizedTickets = tickets.OrderBy(item => item.Priority);
            return Task.FromResult(string.Join("\n--\n", prioritizedTickets.Select(item => $"* title : {item.Title} \n * description : {item.Description} \n * status : {item.Status} \n * priority : {item.Priority}")));
        }
        #endregion
    }
}
