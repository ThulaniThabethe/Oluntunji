using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication1.Models;
using WebApplication1.Migrations;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Initialize database with error handling
            try
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookstoreDbContext, Configuration>());
                
                // Force database initialization
                using (var context = new BookstoreDbContext())
                {
                    if (!context.Database.Exists())
                    {
                        context.Database.Initialize(true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't prevent application startup
                System.Diagnostics.EventLog.WriteEntry("Application", $"Database initialization failed: {ex.Message}", System.Diagnostics.EventLogEntryType.Warning);
            }
            
            // Debug: Log application startup with timestamp - Force recompilation v2
            System.Diagnostics.Debug.WriteLine("Application started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            System.Diagnostics.Debug.WriteLine("Force recompilation v2: " + Guid.NewGuid().ToString());
            System.Diagnostics.Debug.WriteLine("Database connection string: " + System.Configuration.ConfigurationManager.ConnectionStrings["BookstoreConnection"]?.ConnectionString);
        }
    }
}
