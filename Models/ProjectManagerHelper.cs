using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerProject.Models
{
    public class ProjectManagerHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public Project FindProject(int Id)
        {
            Project project = db.Projects.FirstOrDefault(p => p.Id == Id);
            return project;
        }
        public bool CreateProject(Project newProject)
        {
            if (newProject != null)
            {
                db.Projects.Add(newProject);
                db.SaveChanges();
                return true;
            }
            return false;
        }
        public bool DeleteProject(int Id)
        {
            Project project = FindProject(Id);
            if (project != null)
            {
                db.Projects.Remove(project);
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}