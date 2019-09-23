using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Changed { get; set; }
        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}