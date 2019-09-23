namespace BugTrackerProject.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using BugTrackerProject.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTrackerProject.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "BugTrackerProject.Models.ApplicationDbContext";
        }

        protected override void Seed(BugTrackerProject.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            RoleManager.Create(new IdentityRole("Project Manager"));
            RoleManager.Create(new IdentityRole("Admin"));
            RoleManager.Create(new IdentityRole("Developer"));
            RoleManager.Create(new IdentityRole("Submitter"));
            context.TicketTypes.AddOrUpdate(new TicketType { Name = "System" }, new TicketType { Name = "Program" }, 
            new TicketType { Name = "Method" }, new TicketType { Name = "User Interface" });
            context.TicketStatuses.AddOrUpdate(new TicketStatus { Name = "Created" }, new TicketStatus { Name = "Assigned" },
                new TicketStatus { Name = "Developing" }, new TicketStatus { Name = "Testing" }, new TicketStatus { Name = "Finished" });
            context.TicketPriorities.AddOrUpdate(new TicketPriority { Name = "low" }, new TicketPriority { Name = "Medium" }, new TicketPriority { Name = "High" });
            ApplicationUser pmUser = new ApplicationUser { Email = "Keegan@test.com", UserName = "Keegan@test.com" };
            ApplicationUser dUser = new ApplicationUser { Email = "Joe@test.com", UserName = "Joe@test.com" };
            ApplicationUser SUser = new ApplicationUser { Email = "Bob@test.com", UserName = "Bob@test.com" };
            ApplicationUser AUser = new ApplicationUser { Email = "Admin@test.com", UserName = "Admin@test.com" };
            if (!context.Users.Any(c => c.UserName == pmUser.UserName))
            {
                UserManager.Create(pmUser, "EntityFr@mew0rk");
                UserManager.Create(dUser, "EntityFr@mew0rk");
                UserManager.Create(SUser, "EntityFr@mew0rk");
                UserManager.Create(AUser, "EntityFr@mew0rk");
                UserManager.AddToRole(pmUser.Id, "Project Manager");
                UserManager.AddToRole(dUser.Id, "Developer");
                UserManager.AddToRole(SUser.Id, "Submitter");
                UserManager.AddToRole(AUser.Id, "Developer");
            }
        }
    }
}
