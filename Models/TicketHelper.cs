using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class TicketHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Ticket GetTicket(int ticketId)
        {
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId);
            return ticket;
        }
    }
}