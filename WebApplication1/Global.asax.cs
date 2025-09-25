using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
