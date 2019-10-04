using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class NotificationHelper
    {
        private UserManagerHelper userManagerHelper = new UserManagerHelper();
        private TicketHelper ticketHelper = new TicketHelper();
        public TicketNotification CreateNotification(int ticketId)
        {
            var ticket = ticketHelper.GetTicket(ticketId);
            if(ticket == null)
            {
                return null;
            }
            TicketNotification ticketNotification = new TicketNotification { ApplicationUserId = ticket.AssignToUserId, TicketId = ticketId };
            return ticketNotification;
        }
    }
}