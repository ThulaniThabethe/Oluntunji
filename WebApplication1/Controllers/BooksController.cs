using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Web.Script.Serialization;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class BooksController : BaseController
    {
        // GET: Books/Browse - Alias for Index action
        [AllowAnonymous]
        public ActionResult Browse()
        {
            // Force recompilation - timestamp: 2025-01-01 13:00:00
            return Content("Browse action is working!");
        }

        // GET: Books
        [AllowAnonymous]
        public ActionResult Index(string searchString, string category, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParam = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.PriceSortParam = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.AuthorSortParam = sortOrder == "Author" ? "author_desc" : "Author";

            // Load books without navigation properties to avoid notification column issues
            var books = Db.Books.Where(b => b.IsAvailable);

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
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Load book with navigation properties, explicitly selecting only needed fields
            var bookQuery = from b in Db.Books
                           where b.BookId == id
                           select new
                           {
                               Book = b,
                               Seller = new
                               {
                                   UserId = b.Seller.UserId,
                                   FirstName = b.Seller.FirstName,
                                   LastName = b.Seller.LastName,
                                   Email = b.Seller.Email,
                                   Username = b.Seller.Username,
                                   Role = b.Seller.Role,
                                   PhoneNumber = b.Seller.PhoneNumber,
                                   Address = b.Seller.Address,
                                   City = b.Seller.City,
                                   Province = b.Seller.Province,
                                   PostalCode = b.Seller.PostalCode,
                                   EmailConfirmed = b.Seller.EmailConfirmed,
                                   IsActive = b.Seller.IsActive,
                                   CreatedDate = b.Seller.CreatedDate,
                                   LastLoginDate = b.Seller.LastLoginDate
                               },
                               Reviews = b.Reviews.Where(r => r.IsApproved).Select(r => new
                               {
                                   ReviewId = r.ReviewId,
                                   Rating = r.Rating,
                                   ReviewText = r.ReviewText,
                                   ReviewDate = r.ReviewDate,
                                   IsApproved = r.IsApproved,
                                   Customer = new
                                   {
                                       UserId = r.Customer.UserId,
                                       FirstName = r.Customer.FirstName,
                                       LastName = r.Customer.LastName,
                                       Email = r.Customer.Email,
                                       Username = r.Customer.Username,
                                       Role = r.Customer.Role,
                                       PhoneNumber = r.Customer.PhoneNumber,
                                       Address = r.Customer.Address,
                                       City = r.Customer.City,
                                       Province = r.Customer.Province,
                                       PostalCode = r.Customer.PostalCode,
                                       EmailConfirmed = r.Customer.EmailConfirmed,
                                       IsActive = r.Customer.IsActive,
                                       CreatedDate = r.Customer.CreatedDate,
                                       LastLoginDate = r.Customer.LastLoginDate
                                   }
                               }).ToList()
                           };

            var bookData = bookQuery.FirstOrDefault();
            if (bookData == null)
            {
                return HttpNotFound();
            }

            // Manually map the data back to avoid notification column issues
            var book = bookData.Book;
            
            // Create a new User object for Seller with only the needed properties
            book.Seller = new User
            {
                UserId = bookData.Seller.UserId,
                FirstName = bookData.Seller.FirstName,
                LastName = bookData.Seller.LastName,
                Email = bookData.Seller.Email,
                Username = bookData.Seller.Username,
                Role = bookData.Seller.Role,
                PhoneNumber = bookData.Seller.PhoneNumber,
                Address = bookData.Seller.Address,
                City = bookData.Seller.City,
                Province = bookData.Seller.Province,
                PostalCode = bookData.Seller.PostalCode,
                EmailConfirmed = bookData.Seller.EmailConfirmed,
                IsActive = bookData.Seller.IsActive,
                CreatedDate = bookData.Seller.CreatedDate,
                LastLoginDate = bookData.Seller.LastLoginDate
            };

            // Create Review objects with Customer data
            book.Reviews = new List<Review>();
            foreach (var reviewData in bookData.Reviews)
            {
                var review = new Review
                {
                    ReviewId = reviewData.ReviewId,
                    Rating = reviewData.Rating,
                    ReviewText = reviewData.ReviewText,
                    ReviewDate = reviewData.ReviewDate,
                    IsApproved = reviewData.IsApproved,
                    BookId = book.BookId,
                    CustomerId = reviewData.Customer.UserId,
                    Customer = new User
                    {
                        UserId = reviewData.Customer.UserId,
                        FirstName = reviewData.Customer.FirstName,
                        LastName = reviewData.Customer.LastName,
                        Email = reviewData.Customer.Email,
                        Username = reviewData.Customer.Username,
                        Role = reviewData.Customer.Role,
                        PhoneNumber = reviewData.Customer.PhoneNumber,
                        Address = reviewData.Customer.Address,
                        City = reviewData.Customer.City,
                        Province = reviewData.Customer.Province,
                        PostalCode = reviewData.Customer.PostalCode,
                        EmailConfirmed = reviewData.Customer.EmailConfirmed,
                        IsActive = reviewData.Customer.IsActive,
                        CreatedDate = reviewData.Customer.CreatedDate,
                        LastLoginDate = reviewData.Customer.LastLoginDate
                    }
                };
                book.Reviews.Add(review);
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

            Book book = Db.Books.FirstOrDefault(b => b.BookId == id);
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

        // POST: Books/DeleteBook/5 (Alias for MyBooks page)
        [HttpPost]
        [Authorize]
        [AdminOrSeller]
        public ActionResult DeleteBook(int id)
        {
            try
            {
                Book book = Db.Books.Find(id);
                if (book != null)
                {
                    // Check if user has permission to delete this book
                    var currentUser = CurrentUser;
                    if (currentUser.Role != "Admin" && book.SellerId != currentUser.UserId)
                    {
                        return Json(new { success = false, message = "You don't have permission to delete this book." });
                    }

                    Db.Books.Remove(book);
                    Db.SaveChanges();
                    
                    return Json(new { success = true, message = "Book deleted successfully." });
                }
                return Json(new { success = false, message = "Book not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting book: " + ex.Message });
            }
        }

        // GET: Books/Manage
        [AdminOrSeller]
        public ActionResult Manage()
        {
            var currentUser = CurrentUser;
            var books = currentUser.Role == "Admin" 
                ? Db.Books.ToList()
                : Db.Books.Where(b => b.SellerId == currentUser.UserId).ToList();

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