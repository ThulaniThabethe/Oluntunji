using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TestController : Controller
    {
        private BookstoreDbContext db = new BookstoreDbContext();

        public ActionResult Index()
        {
            return Content("<h1>Test Controller Working</h1><p>Application is running successfully.</p>", "text/html");
        }

        public ActionResult Database()
        {
            try
            {
                var bookCount = db.Books.Count();
                return Content($"<h1>Database Test</h1><p>Books in database: {bookCount}</p>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<h1>Database Error</h1><p>Error: {ex.Message}</p><p>Stack: {ex.StackTrace}</p>", "text/html");
            }
        }
    }
}