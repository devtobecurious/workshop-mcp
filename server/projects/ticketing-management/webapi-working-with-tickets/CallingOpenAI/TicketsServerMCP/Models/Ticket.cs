using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsServerMCP.Models
{
    public record Ticket(string Title, string Description, string Status);

    public static class TicketDatabase
    {
        private static readonly List<Ticket> tickets = new()
        {
            new Ticket("Login Issue", "User cannot log in to the system.", "Open"),
            new Ticket("Page Load Error", "The dashboard page fails to load.", "In Progress"),
            new Ticket("Feature Request", "Add dark mode to the application.", "Closed"),
        };
        public static IEnumerable<Ticket> GetAllTickets() => tickets;
    }
}
