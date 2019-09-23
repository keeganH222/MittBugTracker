using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class TicketType
    {
        public TicketType()
        {
            this.Tickets = new List<Ticket>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}