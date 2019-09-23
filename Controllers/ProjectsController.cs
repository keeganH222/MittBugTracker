using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTrackerProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTrackerProject.Controllers
{
    [Authorize(Roles = "Project Manager, Admin")]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectManagerHelper projectManager = new ProjectManagerHelper();
        private UserManagerHelper userManagerHelper = new UserManagerHelper();
        private UserManager<ApplicationUser> userManager;
        public ProjectsController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }
        // GET: Projects
        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }
        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("AssignUsersToProjects", new { projectId = project.Id });
            }

            return View(project);
        }
        public ActionResult AssignUsersToProjects(int projectId)
        {
            var project = projectManager.FindProject(projectId);
            if(project == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userList = db.Users.ToList();
            List<ApplicationUser> developers = new List<ApplicationUser>();
            foreach (var developer in userList)
            {
                if (userManager.IsInRole(developer.Id, "Developer"))
                {
                    developers.Add(developer);
                }
            }
            ViewBag.UserIdArr = new MultiSelectList(developers, "Id", "UserName");
            return View();
        }
        [HttpPost]
        public ActionResult AssignUsersToProjects(int projectId, string[] userIdArr)
        {
            var project = projectManager.FindProject(projectId);
            if (project == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (userIdArr != null && userIdArr.Count() > 0)
            {
                foreach(var userId in userIdArr)
                {
                    var user = userManagerHelper.FindUser(userId);
                    if(user != null)
                    {
                        ProjectUsers newProjectUsers = new ProjectUsers { ProjectId = project.Id, ApplicationUserId = userId };
                        db.ProjectUsers.Add(newProjectUsers);
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }
        public ActionResult RemoveUsersFromProjects(int projectId)
        {
            var project = projectManager.FindProject(projectId);
            if (project == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var developerList = db.ProjectUsers.Where(pu => pu.ProjectId == project.Id);
            ViewBag.devList = new MultiSelectList(developerList, "Id", "UserName");
            return View();
        }
        [HttpPost]
        public ActionResult RemoveUsersFromProjects(int projectId, string[] devList)
        {
            var project = projectManager.FindProject(projectId);
            if (project == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var developerList = db.ProjectUsers.Where(pu => pu.ProjectId == project.Id);
            ViewBag.devList = new MultiSelectList(developerList, "Id", "UserName");
            if (devList != null && devList.Count() > 0)
            {
                foreach (var userId in devList)
                {
                    var user = userManagerHelper.FindUser(userId);
                    if (user != null)
                    {
                        ProjectUsers removeProjectUser = db.ProjectUsers.FirstOrDefault(pu => pu.ApplicationUserId == userId);
                        if(removeProjectUser != null)
                        {
                            db.ProjectUsers.Remove(removeProjectUser);
                            db.SaveChanges();
                        }
                    }
                }
            }
            return View();
        }
        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        
    }
}
