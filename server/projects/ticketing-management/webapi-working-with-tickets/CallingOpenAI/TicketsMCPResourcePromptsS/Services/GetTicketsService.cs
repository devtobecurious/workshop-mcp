using TicketsMCPResourcePromptsS.Models;

namespace TicketsMCPResourcePromptsS.Services
{
    public class GetTicketsService
    {
        public IEnumerable<Ticket> GetAll()
        {
            return TicketDatabase.GetAllTickets();
        }
    }
}
