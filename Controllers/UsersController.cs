using BugTrackerProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace BugTrackerProject.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManagerHelper userManagerHelper = new UserManagerHelper();
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        public UsersController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
        }
        
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Submitter")]
        public ActionResult CreateTicket()
        {
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult CreateTicket([Bind(Include = "Id, Title, Description, Created, Updated, ProjectId, TicketPriorityId, TicketTypeId")] Ticket newTicket)
        {
            var userId = User.Identity.GetUserId();
            var user = userManagerHelper.FindUser(userId);
            var ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Created");
            if(user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            newTicket.OwnerUserId = user.Id;
            newTicket.TicketStatusId = ticketStatus.Id;
            db.Tickets.Add(newTicket);
            db.SaveChanges();
            return RedirectToAction("SubmitterTickets", new { userId = user.Id });
        }
        public ActionResult AssignUserToTicket(int ticketId)
        {
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if(ticket == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var devList = db.Users.Where(u => userManager.IsInRole(u.Id, "Developer")).ToList();
            ViewBag.developerId = new SelectList(devList, "Id", "UserName");
            return View();
        }
        [HttpPost]
        public ActionResult AssignUserToTicket(int ticketId, string developerId)
        {
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId);
            var user = userManagerHelper.FindUser(developerId);
            if (ticket == null || user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var devList = db.Users.Where(u => userManager.IsInRole(u.Id, "Developer")).ToList();
            ViewBag.developer = new SelectList(devList, "Id", "UserName");
            ticket.AssignToUserId = developerId;
            db.Entry(ticket).State = EntityState.Modified;
            db.SaveChanges();
            return View();
        }
        public ActionResult EditTicket(int ticketId)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if(User.Identity.AuthenticationType == "Developer")
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
                ViewBag.Developer = true;
            }
            else if(User.Identity.AuthenticationType == "Project Manager" || User.Identity.AuthenticationType == "Admin")
            {
                var devList = db.Users.Where(u => userManager.IsInRole(u.Id, "Developer")).ToList();
                ViewBag.AssignToUserId = new SelectList(devList, "Id", "UserName");
            }
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult EditTicket(int ticketId, [Bind(Include = "Id,Title,Description,Created,Updated,TicketTypeId,TicketPriorityId,TicketStatusId, AssignToUserId")] Ticket ticket)
        {
            Ticket editTicket = db.Tickets.Find(ticketId);
            if (editTicket == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            if (User.Identity.AuthenticationType == "Developer")
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
                ViewBag.Developer = true;
            }
            else if (User.Identity.AuthenticationType == "Project Manager" || User.Identity.AuthenticationType == "Admin")
            {
                var devList = db.Users.Where(u => userManager.IsInRole(u.Id, "Developer")).ToList();
                ViewBag.AssignToUserId = new SelectList(devList, "Id", "UserName");
            }
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            
            return View(ticket);
        }
        public ActionResult AddComment(int ticketId)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.ShouldUserAddComment = false;
            return View();
        }
        [HttpPost]
        public ActionResult AddComment(int ticketId, bool ShouldUserAddComment, [Bind (Include = "Id, Comment")] TicketComment newComment)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if(ShouldUserAddComment)
            {
                var userId = User.Identity.GetUserId();
                newComment.Created = DateTime.Now;
                newComment.TicketId = ticket.Id;
                newComment.ApplicationUserId = userId;
                db.TicketComments.Add(newComment);
                db.SaveChanges();
                return View();
            }
            else
            {
                return View();
            }
        }
        [Authorize]
        public ActionResult AddAttachment(int ticketId)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.fileDescript = "";
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult AddAttachment(int ticketId, HttpPostedFile file, string fileDescript)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (file != null || file.ContentLength > 0)
            {
                var fileName = file.FileName;
                var path = Path.Combine(Server.MapPath("~/App_Data/Upload"), fileName);
                file.SaveAs(path);
                var userId = User.Identity.GetUserId();
                TicketAttachment newAttachment = new TicketAttachment { FilePath = path, FileUrl = "~/ App_Data / Upload", Created = DateTime.Now, Description = fileDescript };
                newAttachment.TicketId = ticket.Id;
                newAttachment.ApplicationUserId = userId;
            }
            return View();
            
        }
    }
}