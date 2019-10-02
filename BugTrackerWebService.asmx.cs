using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using BugTrackerProject.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace BugTrackerProject
{
    /// <summary>
    /// Summary description for BugTrackerWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    //To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.

    [System.Web.Script.Services.ScriptService]
    public class BugTrackerWebService : System.Web.Services.WebService
    {
        private UserManagerHelper userManagerHelper = new UserManagerHelper();
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> userManager;
        public BugTrackerWebService()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }
        [WebMethod]
        public List<Ticket> GetTickets(List<string> input)
        {
            var ticketList = db.Tickets.ToList();
            String userId = "";
            String role = "";
            int index = 0;
            foreach(var item in input)
            {
                if(index == 0)
                {
                    userId = item;
                }
                else
                {
                    role = item;
                }
                index++;
            }
            var user = userManagerHelper.FindUser(userId);
            if (userManager.IsInRole(userId, "Admin") || user.UserName == "DemoUser40@outlook.com")
            {
                return ticketList;
            }
            else if (userManager.IsInRole(userId, "Project Manager"))
            {
                var myProjects = (from projectUser in db.ProjectUsers
                                  where projectUser.ApplicationUserId == userId
                                  select projectUser).ToList();
                var tickets = (from ticket in ticketList
                               join project in myProjects
                               on ticket.ProjectId equals project.Id
                               select ticket).ToList();
                return tickets;
            }
            else if (userManager.IsInRole(userId, "Developer"))
            {
                var myProjects = (from projectUser in db.ProjectUsers
                                  where projectUser.ApplicationUserId == userId
                                  select projectUser).ToList();
                var tickets = (from ticket in ticketList
                               join project in myProjects
                               on ticket.ProjectId equals project.Id
                               select ticket).ToList();
                return tickets;
            }
            else if (userManager.IsInRole(userId, "Submitter"))
            {
                var submitter = userManagerHelper.FindUser(userId);
                if (submitter == null)
                {
                    return null;
                }
                var tickets = user.OwnedTickets.ToList();
                return tickets;
            }
            else
            {
                return null;
            }
        }
    }
}
