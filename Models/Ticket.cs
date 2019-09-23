using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class Ticket
    {
        public Ticket()
        {
            this.TicketAttachments = new List<TicketAttachment>();
            this.TicketComments = new List<TicketComment>();
            this.TicketHistories = new List<TicketHistory>();
            this.TicketNotifications = new List<TicketNotification>();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string Title { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public bool Updated { get; set; }
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        [ForeignKey("TicketType")]
        public int TicketTypeId { get; set; }
        public virtual TicketType TicketType { get; set; }
        [ForeignKey("TicketPriority")]
        public int TicketPriorityId { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        [ForeignKey("TicketStatus")]
        public int TicketStatusId { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }

        [ForeignKey("OwnerUser")]
        public string OwnerUserId { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }

        [ForeignKey("AssignToUser")]
        public string AssignToUserId { get; set; }
        public virtual ApplicationUser AssignToUser { get; set; }
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; }
        public virtual ICollection<TicketComment> TicketComments { get; set; }
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }
        public virtual ICollection<TicketNotification> TicketNotifications { get; set; }

    }
}