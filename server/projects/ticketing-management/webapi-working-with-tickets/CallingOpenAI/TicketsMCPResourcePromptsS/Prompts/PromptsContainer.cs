using Microsoft.Extensions.AI;

using ModelContextProtocol.Server;

using System.ComponentModel;

namespace TicketsMCPResourcePromptsS.Prompts
{
    [McpServerPromptType]
    public class PromptsContainer
    {
        #region Public methods
        [McpServerPrompt]
        [Description("Summarizes the provided text in a single line.")]
        public ChatMessage Summarize([Description("The text to summarize.")] string text)
        {
            return new ChatMessage(ChatRole.User, $"Please summarize the following text: {text} in single line");
        }

        [McpServerPrompt]
        [Description("Génère un rapport de tickets pour une période données")]
        public ChatMessage GenerateTicketsReport([Description("Date de début (yyyy-MM-dd")] string startDate, [Description("Date de fin (yyyy-MM-dd)")] string endDate)
        {
            return new(
                ChatRole.User,
                $"Génère un rapport de tickets entre les dates {startDate} et {endDate}. 1. Utilise summarize pour avoir un résumé de chaque ticket. Le rapport doit inclure le nombre total de tickets, le nombre de tickets résolus, le nombre de tickets en cours et une liste des 5 principaux problèmes signalés."
            );
        }

        #endregion
    }
}
