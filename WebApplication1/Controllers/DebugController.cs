using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class DebugController : Controller
    {
        // GET: Debug/GetDebugInfo
        [AllowAnonymous]
        public ActionResult GetDebugInfo()
        {
            var debugInfo = TempData["DebugInfo"] as string;
            if (string.IsNullOrEmpty(debugInfo))
            {
                debugInfo = "No debug information available. Make sure to call /Account/Register first.";
            }
            
            return Content(debugInfo, "text/plain");
        }
        
        // GET: Debug/ClearDebugInfo
        [AllowAnonymous]
        public ActionResult ClearDebugInfo()
        {
            TempData["DebugInfo"] = null;
            return Content("Debug information cleared.", "text/plain");
        }
        
        // GET: Debug/ForceError
        [AllowAnonymous]
        public ActionResult ForceError()
        {
            throw new Exception("This is a forced error for testing purposes.");
        }
        
        // GET: Debug/TestDatabase
        [AllowAnonymous]
        public ActionResult TestDatabase()
        {
            try
            {
                using (var db = new Models.BookstoreDbContext())
                {
                    // Try to access the Users table
                    var userCount = db.Users.Count();
                    return Content($"Database connection successful. User count: {userCount}", "text/plain");
                }
            }
            catch (Exception ex)
            {
                return Content($"Database connection failed: {ex.Message}\n{ex.StackTrace}", "text/plain");
            }
        }
        
        // GET: Debug/LogToFile
        [AllowAnonymous]
        public ActionResult LogToFile(string message)
        {
            try
            {
                var logPath = Server.MapPath("~/debug-log.txt");
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n";
                System.IO.File.AppendAllText(logPath, logMessage);
                return Content($"Message logged to: {logPath}", "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"Failed to log message: {ex.Message}", "text/plain");
            }
        }
        
        // GET: Debug/ReadLog
        [AllowAnonymous]
        public ActionResult ReadLog()
        {
            try
            {
                var logPath = Server.MapPath("~/debug-log.txt");
                if (System.IO.File.Exists(logPath))
                {
                    var content = System.IO.File.ReadAllText(logPath);
                    return Content(content, "text/plain");
                }
                else
                {
                    return Content("No log file found.", "text/plain");
                }
            }
            catch (Exception ex)
            {
                return Content($"Failed to read log: {ex.Message}", "text/plain");
            }
        }
    }
}