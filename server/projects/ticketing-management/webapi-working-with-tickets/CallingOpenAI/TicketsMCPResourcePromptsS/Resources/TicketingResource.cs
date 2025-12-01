using ModelContextProtocol.Server;

namespace TicketsMCPResourcePromptsS.Resources
{
    [McpServerResourceType()]
    public class TicketingResource
    {
        private readonly string documentationPath = string.Empty;

        public TicketingResource(IConfiguration configuration)
        {
            documentationPath = configuration["Documentation:Path"] ?? string.Empty;
        }

        [McpServerResource(MimeType = "text/markdown", UriTemplate = "docs://escalation-procedures")]
        public async Task<string> GetReglementation()
        {
            using StreamReader reader = new StreamReader(Path.Combine(documentationPath, "reglementation.md"));
            string content = await reader.ReadToEndAsync().ConfigureAwait(false);
            return content;
        }
    }
}
