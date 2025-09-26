using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BookController : Controller
    {
        private BookstoreDbContext Db = new BookstoreDbContext();

        // GET: Book/Browse
        public ActionResult Browse(string searchString, string category, string sortOrder)
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
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = category;
            
            return View(books.ToList());
        }

        // GET: Book/Details/5
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

        // GET: Book/WriteReview/5
        [Authorize]
        public ActionResult WriteReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var book = Db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            // Check if user has already reviewed this book
            var currentUserId = GetCurrentUserId();
            var existingReview = Db.Reviews.FirstOrDefault(r => r.BookId == id && r.CustomerId == currentUserId);
            
            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this book.";
                return RedirectToAction("Details", new { id = id });
            }

            ViewBag.BookId = id;
            ViewBag.BookTitle = book.Title;
            return View();
        }

        // POST: Book/WriteReview/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult WriteReview(int id, FormCollection form)
        {
            try
            {
                int rating = int.Parse(form["Rating"]);
                string reviewText = form["ReviewText"];

                var review = new Review
                {
                    BookId = id,
                    CustomerId = GetCurrentUserId(),
                    Rating = rating,
                    ReviewText = reviewText,
                    ReviewDate = DateTime.Now,
                    IsApproved = false // Reviews need approval by default
                };

                Db.Reviews.Add(review);
                Db.SaveChanges();

                TempData["Success"] = "Your review has been submitted and will be reviewed by the seller.";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error submitting review: " + ex.Message;
                return RedirectToAction("Details", new { id = id });
            }
        }

        private int GetCurrentUserId()
        {
            // Get the current user's ID from the authentication system
            var username = User.Identity.Name;
            var user = Db.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserId ?? 0;
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