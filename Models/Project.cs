using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class Project
    {
        public Project()
        {
            this.ProjectUsers = new List<ProjectUsers>();
            this.Tickets = new List<Ticket>();
        }
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string Name { get; set; }
        public virtual ICollection<ProjectUsers> ProjectUsers { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}