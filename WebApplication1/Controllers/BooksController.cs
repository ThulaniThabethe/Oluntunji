using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class BooksController : BaseController
    {
        // GET: Books
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
            return View(books.ToList());
        }

        // GET: Books/Details/5
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

        // GET: Books/Create
        [AdminOrSeller]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOrSeller]
        public ActionResult Create([Bind(Include = "Title,Author,Publisher,ISBN,PublicationYear,Description,Price,StockQuantity,Category,Genre,CoverImageUrl")] Book book)
        {
            if (ModelState.IsValid)
            {
                var currentUser = CurrentUser;
                book.SellerId = currentUser.UserId;
                book.CreatedDate = DateTime.Now;
                book.IsAvailable = true;
                book.LastUpdatedDate = DateTime.Now;

                Db.Books.Add(book);
                Db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(book);
        }

        // GET: Books/Edit/5
        [AdminOrSeller]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Book book = Db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            // Check if user has permission to edit this book
            var currentUser = CurrentUser;
            if (currentUser.Role != "Admin" && book.SellerId != currentUser.UserId)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOrSeller]
        public ActionResult Edit([Bind(Include = "BookId,Title,Author,Publisher,ISBN,PublicationYear,Description,Price,StockQuantity,Category,Genre,CoverImageUrl,IsAvailable,IsFeatured")] Book book)
        {
            if (ModelState.IsValid)
            {
                var existingBook = Db.Books.Find(book.BookId);
                if (existingBook == null)
                {
                    return HttpNotFound();
                }

                // Check if user has permission to edit this book
                var currentUser = CurrentUser;
                if (currentUser.Role != "Admin" && existingBook.SellerId != currentUser.UserId)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                // Update properties
                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.Publisher = book.Publisher;
                existingBook.ISBN = book.ISBN;
                existingBook.PublicationYear = book.PublicationYear;
                existingBook.Description = book.Description;
                existingBook.Price = book.Price;
                existingBook.StockQuantity = book.StockQuantity;
                existingBook.Category = book.Category;
                existingBook.Genre = book.Genre;
                existingBook.CoverImageUrl = book.CoverImageUrl;
                existingBook.IsAvailable = book.IsAvailable;
                existingBook.IsFeatured = book.IsFeatured;
                existingBook.LastUpdatedDate = DateTime.Now;

                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(book);
        }

        // GET: Books/Delete/5
        [AdminOrSeller]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Book book = Db.Books.Include(b => b.Seller).FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                return HttpNotFound();
            }

            // Check if user has permission to delete this book
            var currentUser = CurrentUser;
            if (currentUser.Role != "Admin" && book.SellerId != currentUser.UserId)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOrSeller]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = Db.Books.Find(id);
            if (book != null)
            {
                // Check if user has permission to delete this book
                var currentUser = CurrentUser;
                if (currentUser.Role != "Admin" && book.SellerId != currentUser.UserId)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                Db.Books.Remove(book);
                Db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: Books/Manage
        [AdminOrSeller]
        public ActionResult Manage()
        {
            var currentUser = CurrentUser;
            var books = currentUser.Role == "Admin" 
                ? Db.Books.Include(b => b.Seller).ToList()
                : Db.Books.Where(b => b.SellerId == currentUser.UserId).Include(b => b.Seller).ToList();

            return View(books);
        }

        // GET: Books/LowStock
        [AdminOrSeller]
        public ActionResult LowStock()
        {
            var currentUser = CurrentUser;
            var lowStockBooks = currentUser.Role == "Admin"
                ? Db.Books.Where(b => b.StockQuantity <= 5 && b.IsAvailable).ToList()
                : Db.Books.Where(b => b.SellerId == currentUser.UserId && b.StockQuantity <= 5 && b.IsAvailable).ToList();

            return View(lowStockBooks);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}