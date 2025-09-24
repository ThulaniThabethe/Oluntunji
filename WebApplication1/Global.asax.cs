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
        }
    }
}
