﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class TicketPriority
    {
        public TicketPriority()
        {
            this.Tickets = new List<Ticket>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}