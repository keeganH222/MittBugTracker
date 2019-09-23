using BugTrackerProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManagerHelper userManagerHelper = new UserManagerHelper();
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        public AdminsController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
        }
        // GET: Admins
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AssignToRoles()
        {
            var userList = db.Users.ToList();
            var roleList = db.Roles.ToList();
            ViewBag.userId = new SelectList(userList, "Id", "UserName");
            ViewBag.roleName = new SelectList(roleList, "RoleName", "RoleName");
            return View();
        }
        [HttpPost]
        public ActionResult AssignToRoles(string userId, string roleName)
        {
            var user = userManagerHelper.FindUser(userId);
            StringBuilder returnedString = new StringBuilder();
            if(user != null)
            {
                if(userManagerHelper.AddToRole(userId, roleName))
                {
                    returnedString.Append(string.Format("{0} was added to {1} role!", user.UserName, roleName));
                    ViewBag.Message = returnedString.ToString();
                }
                else
                {
                    returnedString.Append(string.Format("{0} was not added to {1} role!", user.UserName, roleName));
                    ViewBag.Message = returnedString.ToString();
                }
                return View();
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public ActionResult SelectUserToRemove()
        {
            var userList = db.Users.ToList();
            ViewBag.userId = new SelectList(userList, "Id", "UserName");
            return View();
        }
        [HttpPost]
        public ActionResult SelectUserToRemove(string userId)
        {
            var user = userManagerHelper.FindUser(userId);
            if(user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return RedirectToAction("RemoveUserFromRole", new { userId = userId });
        }
        public ActionResult RemoveUserFromRole(string userId)
        {
            var user = userManagerHelper.FindUser(userId);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var roleList = user.Roles.ToList();
            ViewBag.roleName = new SelectList(roleList, "Name", "Name");
            return View();
        }
        [HttpPost]
        public ActionResult RemoveUserFromRole(string userId, string roleName)
        {
            var user = userManagerHelper.FindUser(userId);
            StringBuilder returnedString = new StringBuilder();
            if (user == null || roleManager.RoleExists(roleName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var roleList = user.Roles.ToList();
            ViewBag.roleName = new SelectList(roleList, "Name", "Name");
            if(userManagerHelper.RemoveFromRole(userId, roleName))
            {
                returnedString.Append(string.Format("{0} was removed from {1} role!",user.UserName, roleName));
            }
            else
            {
                returnedString.Append(string.Format("{0} was not removed from {1} role!", user.UserName, roleName));
            }
            ViewBag.Message = returnedString.ToString();
            return View();
        }
    }
}