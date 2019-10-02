using BugTrackerProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
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
        private ProjectManagerHelper projectManagerHelper = new ProjectManagerHelper();
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private ApplicationSignInManager _signInManager;
        public UsersController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
        }
        public UsersController(ApplicationSignInManager signInManager)
        {
            SignInManager = signInManager;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult CreateTicket()
        {
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult CreateTicket([Bind(Include = "Id, Title, Description, ProjectId, TicketPriorityId, TicketTypeId")] Ticket newTicket)
        {
            var userId = User.Identity.GetUserId();
            var user = userManagerHelper.FindUser(userId);
            var ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Created");
            newTicket.Created = DateTime.Now;
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            newTicket.OwnerUserId = user.Id;
            newTicket.TicketStatusId = ticketStatus.Id;
            db.Tickets.Add(newTicket);
            db.SaveChanges();
            return RedirectToAction("ListOfTickets");
        }
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUserToTicket(int ticketId)
        {
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null)
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
            if (User.IsInRole("Developer"))
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
                ViewBag.Developer = true;
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Admin"))
            {
                var devList = db.Users.Where(u => userManager.IsInRole(u.Id, "Developer")).ToList();
                ViewBag.AssignToUserId = new SelectList(devList, "Id", "UserName");
                ViewBag.Manager = true;
            }
            else if (User.IsInRole("Submitter"))
            {
                ViewBag.Submitter = true;
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
                return RedirectToAction("AddComment");
            }
            if (User.IsInRole("Developer"))
            {
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
                ViewBag.Developer = true;
            }
            else if (User.IsInRole("Project Manager") || User.IsInRole("Admin"))
            {
                var devList = db.Users.Where(u => userManager.IsInRole(u.Id, "Developer")).ToList();
                ViewBag.AssignToUserId = new SelectList(devList, "Id", "UserName");
            }
            else if (User.IsInRole("Submitter"))
            {
                ViewBag.Submitter = true;
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
        public ActionResult AddComment(int ticketId, bool ShouldUserAddComment, [Bind(Include = "Id, Comment")] TicketComment newComment)
        {
            Ticket ticket = db.Tickets.Find(ticketId);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            if (ShouldUserAddComment)
            {
                var userId = User.Identity.GetUserId();
                newComment.Created = DateTime.Now;
                newComment.TicketId = ticket.Id;
                newComment.ApplicationUserId = userId;
                db.TicketComments.Add(newComment);
                db.SaveChanges();
                return RedirectToAction("ListOfTickets");
            }
            else
            {
                return RedirectToAction("ListOfTickets");
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
                var fileName = file.FileName + DateTime.Now;
                var path = Path.Combine(Server.MapPath("~/App_Data/Upload"), fileName);
                file.SaveAs(path);
                var userId = User.Identity.GetUserId();
                TicketAttachment newAttachment = new TicketAttachment { FilePath = path, FileUrl = path, Created = DateTime.Now, Description = fileDescript };
                newAttachment.TicketId = ticket.Id;
                newAttachment.ApplicationUserId = userId;
            }
            return RedirectToAction("ListOfTickets");
        }
        [Authorize]
        public ActionResult ListOfTickets()
        {
            var userId = User.Identity.GetUserId();
            var demoUser = userManagerHelper.FindUser(userId);
            List<string> input = new List<string>();
            if (userManager.IsInRole(userId, "Admin") || demoUser.UserName == "DemoUser40@outlook.com")
            {
                ViewBag.Demo = true;
                input.Add(userId);
                input.Add("Admin");
                ViewBag.conditionInput = input;
                return View();
            }
            else if (userManager.IsInRole(userId, "Project Manager"))
            {
                input.Add(userId);
                input.Add("Project Manager");
                ViewBag.conditionInput = input;
                return View();
            }
            else if (userManager.IsInRole(userId, "Developer"))
            {
                input.Add(userId);
                input.Add("Developer");
                ViewBag.conditionInput = input;
                return View();
            }
            else if (userManager.IsInRole(userId, "Submitter"))
            {
                input.Add(userId);
                input.Add("Submitter");
                ViewBag.conditionInput = input;
                ViewBag.Submitter = true;
                return View();
            }
            return HttpNotFound();
        }
        private const int TOTAL_ROWS = 995;
        private static List<DataItem> _data = new List<DataItem>();
        public class DataItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime Created { get; set; }
            public bool Updated { get; set; }
            public string ProjectName { get; set; }
            public string Ticket_Type_Name { get; set; }
            public string Ticket_Priority_Name { get; set; }
            public string Ticket_Status_Name { get; set; }
            public string Assigned_Developer { get; set; }
            public string Ticket_Owner { get; set; }
        }

        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<DataItem> data { get; set; }
        }
        public List<DataItem> getTicketListData(ApplicationUser user)
        {
            var ticketList = db.Tickets.ToList();
            var userId = user.Id;
            List<DataItem> dataItems = new List<DataItem>();
            if (userManager.IsInRole(userId, "Admin") || user.UserName == "DemoUser40@outlook.com")
            {
                foreach(var ticket in ticketList)
                {
                    DataItem dataItemToAdd = new DataItem { Title = ticket.Title, Description = ticket.Description, Created = ticket.Created, Updated = ticket.Updated, ProjectName = ticket.Project.Name, Ticket_Type_Name = ticket.TicketType.Name, Ticket_Priority_Name = ticket.TicketPriority.Name, Ticket_Status_Name = ticket.TicketStatus.Name, Assigned_Developer = ticket.AssignToUser.UserName, Ticket_Owner = ticket.OwnerUser.UserName };
                    dataItems.Add(dataItemToAdd);
                }
                return dataItems;
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
                foreach (var ticket in tickets)
                {
                    DataItem dataItemToAdd = new DataItem { Title = ticket.Title, Description = ticket.Description, Created = ticket.Created, Updated = ticket.Updated, ProjectName = ticket.Project.Name, Ticket_Type_Name = ticket.TicketType.Name, Ticket_Priority_Name = ticket.TicketPriority.Name, Ticket_Status_Name = ticket.TicketStatus.Name, Assigned_Developer = ticket.AssignToUser.UserName, Ticket_Owner = ticket.OwnerUser.UserName };
                    dataItems.Add(dataItemToAdd);
                }
                return dataItems;
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
                foreach (var ticket in tickets)
                {
                    DataItem dataItemToAdd = new DataItem { Title = ticket.Title, Description = ticket.Description, Created = ticket.Created, Updated = ticket.Updated, ProjectName = ticket.Project.Name, Ticket_Type_Name = ticket.TicketType.Name, Ticket_Priority_Name = ticket.TicketPriority.Name, Ticket_Status_Name = ticket.TicketStatus.Name, Assigned_Developer = ticket.AssignToUser.UserName, Ticket_Owner = ticket.OwnerUser.UserName };
                    dataItems.Add(dataItemToAdd);
                }
                return dataItems;
            }
            else if (userManager.IsInRole(userId, "Submitter"))
            {
                var submitter = userManagerHelper.FindUser(userId);
                if (submitter == null)
                {
                    return null;
                }
                var tickets = user.OwnedTickets.ToList();
                foreach (var ticket in tickets)
                {
                    DataItem dataItemToAdd = new DataItem { Title = ticket.Title, Description = ticket.Description, Created = ticket.Created, Updated = ticket.Updated, ProjectName = ticket.Project.Name, Ticket_Type_Name = ticket.TicketType.Name, Ticket_Priority_Name = ticket.TicketPriority.Name, Ticket_Status_Name = ticket.TicketStatus.Name, Assigned_Developer = ticket.AssignToUser.UserName, Ticket_Owner = ticket.OwnerUser.UserName };
                    dataItems.Add(dataItemToAdd);
                }
                return dataItems;
            }
            else
            {
                return null;
            }
        }
        public ActionResult AjaxGetJsonData(int draw, int start, int length)
        {
            var userId = User.Identity.GetUserId();
            var user = userManagerHelper.FindUser(userId);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            _data = getTicketListData(user);
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (length == -1)
            {
                length = TOTAL_ROWS;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = TOTAL_ROWS;
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }
        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = int.Parse(s1);
            int i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }
        private List<DataItem> FilterData(ref int recordFiltered, int start, int length, string search, int sortColumn, string sortDirection)
        {
            List<DataItem> list = new List<DataItem>();
            if (search == null)
            {
                list = _data;
            }
            else
            {
                // simulate search
                foreach (DataItem dataItem in _data)
                {
                    if (dataItem.Title.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Description.ToString().Contains(search.ToUpper()) ||
                        dataItem.Created.ToString().Contains(search.ToUpper()) ||
                        dataItem.ProjectName.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Ticket_Type_Name.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Ticket_Priority_Name.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Ticket_Status_Name.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Assigned_Developer.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Ticket_Owner.ToUpper().Contains(search.ToUpper()))
                    {
                        list.Add(dataItem);
                    }
                }
            }

            // simulate sort
            if (sortColumn == 0)
            {// sort Name
                list.Sort((x, y) => SortString(x.Title, y.Title, sortDirection));
            }
            else if (sortColumn == 1)
            {// sort Age
                list.Sort((x, y) => SortInteger(x.Description, y.Description, sortDirection));
            }
            else if (sortColumn == 2)
            {   // sort DoB
                list.Sort((x, y) => SortDateTime(x.Created.ToString(), y.Created.ToString(), sortDirection));
            }
            else if (sortColumn == 4)
            {   // sort DoB
                list.Sort((x, y) => SortString(x.ProjectName, y.ProjectName, sortDirection));
            }
            else if (sortColumn == 5)
            {   // sort DoB
                list.Sort((x, y) => SortString(x.Ticket_Type_Name, y.Ticket_Type_Name, sortDirection));
            }
            else if (sortColumn == 6)
            {   // sort DoB
                list.Sort((x, y) => SortString(x.Ticket_Priority_Name, y.Ticket_Priority_Name, sortDirection));
            }
            else if (sortColumn == 7)
            {   // sort DoB
                list.Sort((x, y) => SortString(x.Ticket_Status_Name, y.Ticket_Status_Name, sortDirection));
            }
            else if (sortColumn == 8)
            {   // sort DoB
                list.Sort((x, y) => SortString(x.Assigned_Developer, y.Assigned_Developer, sortDirection));
            }
            else if (sortColumn == 9)
            {   // sort DoB
                list.Sort((x, y) => SortString(x.Ticket_Owner, y.Ticket_Owner, sortDirection));
            }

            recordFiltered = list.Count;

            // get just one page of data
            list = list.GetRange(start, Math.Min(length, list.Count - start));

            return list;
        }

        public ActionResult ProjectDetails(int projectId)
        {
            var project = projectManagerHelper.FindProject(projectId);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }
        public ActionResult AppStart()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AppStart(string preventOverload)
        {
            var user = db.Users.First(u => u.UserName == "DemoUser40@outlook.com");
            SignInManager.SignIn(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("ListOfTickets");
        }
    }
}