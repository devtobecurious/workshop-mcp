using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsMCPResourcePromptsS.Models
{
    public record TicketHistory(DateTime Timestamp, string Action, string PerformedBy);

    public enum PriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public record Ticket(string Title, string Description, string Status, PriorityLevel Priority, List<TicketHistory> Histories);

    public static class TicketDatabase
    {
        private static readonly List<Ticket> tickets = new()
        {
            new Ticket(
                "Login page not loading",
                "Users are unable to access the login page. The page shows a blank screen after the latest deployment.",
                "In Progress",
                PriorityLevel.Medium,
                new List<TicketHistory>
                {
                    new TicketHistory(DateTime.Now.AddDays(-5), "Ticket created", "user@example.com"),
                    new TicketHistory(DateTime.Now.AddDays(-4), "Assigned to development team", "support@example.com"),
                    new TicketHistory(DateTime.Now.AddDays(-2), "Investigation started", "dev.john@example.com")
                }
            ),
            new Ticket(
                "Database connection timeout",
                "Production database experiencing frequent connection timeouts during peak hours, affecting multiple services.",
                "Open",
                PriorityLevel.High,
                new List<TicketHistory>
                {
                    new TicketHistory(DateTime.Now.AddHours(-3), "Ticket created", "admin@example.com"),
                    new TicketHistory(DateTime.Now.AddHours(-2), "Marked as high priority", "manager@example.com"),
                    new TicketHistory(DateTime.Now.AddHours(-1), "Database team notified", "support@example.com"),
                    new TicketHistory(DateTime.Now.AddMinutes(-30), "Initial diagnostics completed", "dba.sarah@example.com"),
                    new TicketHistory(DateTime.Now.AddMinutes(-10), "Scaling investigation in progress", "dba.sarah@example.com")
                }
            ),
            new Ticket(
                "Mobile app crash on iOS 17",
                "App crashes immediately after launch on devices running iOS 17. Crash logs indicate memory issue.",
                "Resolved",
                PriorityLevel.Critical,
                new List<TicketHistory>
                {
                    new TicketHistory(DateTime.Now.AddDays(-10), "Ticket created", "mobile.user@example.com"),
                    new TicketHistory(DateTime.Now.AddDays(-9), "Assigned to mobile team", "support@example.com"),
                    new TicketHistory(DateTime.Now.AddDays(-7), "Root cause identified", "dev.maria@example.com"),
                    new TicketHistory(DateTime.Now.AddDays(-2), "Fix deployed to production", "dev.maria@example.com")
                }
            ),
            new Ticket(
                "Email notifications delayed",
                "System email notifications are being sent with a delay of 2-3 hours instead of real-time delivery.",
                "Open",
                PriorityLevel.Low,
                new List<TicketHistory>
                {
                    new TicketHistory(DateTime.Now.AddDays(-1), "Ticket created", "customer@example.com"),
                    new TicketHistory(DateTime.Now.AddHours(-12), "Acknowledged by infrastructure team", "infra.team@example.com")
                }
            )
        };
        public static IEnumerable<Ticket> GetAllTickets() => tickets;
    }
}
