using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class UserManagerHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleManager<IdentityRole> roleManager;
        private UserManager<ApplicationUser> userManager;
        public UserManagerHelper()
        {
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }
        public ApplicationUser FindUser(string id)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == id);
            return user;
        }
        public bool AddToRole(string id, string roleName)
        {
            var user = FindUser(id);
            if(user != null)
            {
                if(roleManager.RoleExists(roleName))
                {
                    if(!userManager.IsInRole(id, roleName))
                    {
                        userManager.AddToRole(user.Id, roleName);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool RemoveFromRole(string id, string roleName)
        {
            var user = FindUser(id);
            if (user != null)
            {
                if (roleManager.RoleExists(roleName))
                { 
                    if(!userManager.IsInRole(id, roleName))
                    {
                        userManager.RemoveFromRole(user.Id, roleName);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool DeleteUser(string id)
        {
            var user = FindUser(id);
            if (user != null)
            {
                userManager.Delete(user);
                return true;
            }
            return false;
        }
    }
}