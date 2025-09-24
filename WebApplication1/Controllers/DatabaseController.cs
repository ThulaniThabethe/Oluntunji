using System;
using System.Data.Entity;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Migrations;

namespace WebApplication1.Controllers
{
    public class DatabaseController : Controller
    {
        // GET: Database/Initialize
        public ActionResult Initialize()
        {
            try
            {
                // Set the database initializer
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookstoreDbContext, Configuration>());
                
                // Force database initialization
                using (var context = new BookstoreDbContext())
                {
                    context.Database.Initialize(true);
                }
                
                ViewBag.Message = "Database initialized successfully!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error initializing database: {ex.Message}";
                ViewBag.Success = false;
            }
            
            return View();
        }
        
        // GET: Database/Reset
        public ActionResult Reset()
        {
            try
            {
                // Drop and recreate the database
                Database.SetInitializer(new DropCreateDatabaseAlways<BookstoreDbContext>());
                
                using (var context = new BookstoreDbContext())
                {
                    context.Database.Initialize(true);
                }
                
                // Re-seed the data
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookstoreDbContext, Configuration>());
                
                ViewBag.Message = "Database reset successfully!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error resetting database: {ex.Message}";
                ViewBag.Success = false;
            }
            
            return View("Initialize");
        }
    }
}