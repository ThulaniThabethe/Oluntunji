using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using WebApplication1.Models;
using WebApplication1.Migrations;
using System.Net;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Initialize database with migrations
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookstoreDbContext, Configuration>());
            
            // Force database initialization
            using (var context = new BookstoreDbContext())
            {
                try
                {
                    context.Database.Initialize(true);
                    System.Diagnostics.Debug.WriteLine("Database initialized successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                    // Continue with application startup even if database initialization fails
                }
            }
            
            // Debug: Log application startup with timestamp - Force recompilation v5
            System.Diagnostics.Debug.WriteLine("Application started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            System.Diagnostics.Debug.WriteLine("Force recompilation v5: " + Guid.NewGuid().ToString());
            
            // Explicitly register Book routes
            RouteTable.Routes.MapRoute(
                name: "BookBrowse",
                url: "Book/Browse",
                defaults: new { controller = "Book", action = "Browse" }
            );
            
            RouteTable.Routes.MapRoute(
                name: "BookDetails",
                url: "Book/Details/{id}",
                defaults: new { controller = "Book", action = "Details", id = UrlParameter.Optional }
            );
        }
    }
}
