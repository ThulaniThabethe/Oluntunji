using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BrowseController : Controller
    {
        private BookstoreDbContext Db = new BookstoreDbContext();

        // GET: Browse
        public ActionResult Index(string searchString, string category, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParam = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.PriceSortParam = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.AuthorSortParam = sortOrder == "Author" ? "author_desc" : "Author";

            var books = Db.Books.Include(b => b.Seller).Where(b => b.IsAvailable);

            // Search functionality
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString) || 
                                         b.Author.Contains(searchString) || 
                                         b.ISBN.Contains(searchString));
            }

            // Category filter
            if (!String.IsNullOrEmpty(category))
            {
                books = books.Where(b => b.Category == category);
            }

            // Sort functionality
            switch (sortOrder)
            {
                case "title_desc":
                    books = books.OrderByDescending(b => b.Title);
                    break;
                case "Price":
                    books = books.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(b => b.Price);
                    break;
                case "Author":
                    books = books.OrderBy(b => b.Author);
                    break;
                case "author_desc":
                    books = books.OrderByDescending(b => b.Author);
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;
            }

            ViewBag.Categories = Db.Books.Where(b => b.IsAvailable).Select(b => b.Category).Distinct().ToList();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = category;
            
            return View(books.ToList());
        }

        // GET: Browse/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Book book = Db.Books.Include(b => b.Seller).Include(b => b.Reviews).FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}