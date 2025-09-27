using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        // GET: Profile
        public ActionResult Index()
        {
            var currentUser = CurrentUser;
            var model = new ProfileViewModel
            {
                UserId = currentUser.UserId,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber,
                Address = currentUser.Address,
                City = currentUser.City,
                Province = currentUser.Province,
                PostalCode = currentUser.PostalCode
            };

            return View(model);
        }

        // GET: Profile/Edit
        public ActionResult Edit()
        {
            var currentUser = CurrentUser;
            var model = new EditProfileViewModel
            {
                UserId = currentUser.UserId,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                PhoneNumber = currentUser.PhoneNumber,
                Address = currentUser.Address,
                City = currentUser.City,
                Province = currentUser.Province,
                PostalCode = currentUser.PostalCode
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = CurrentUser;
            
            // Update user profile
            currentUser.FirstName = model.FirstName;
            currentUser.LastName = model.LastName;
            currentUser.PhoneNumber = model.PhoneNumber;
            currentUser.Address = model.Address;
            currentUser.City = model.City;
            currentUser.Province = model.Province;
            currentUser.PostalCode = model.PostalCode;

            Db.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Profile/ChangePassword
        public ActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            return View(model);
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = CurrentUser;

            // Verify current password
            if (!Helpers.PasswordHelper.VerifyPassword(model.CurrentPassword, currentUser.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Update password
            currentUser.PasswordHash = Helpers.PasswordHelper.HashPassword(model.NewPassword);
            Db.SaveChanges();

            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("Index");
        }

        // GET: Profile/OrderHistory
        public ActionResult OrderHistory()
        {
            var currentUser = CurrentUser;
            var orders = Db.Orders
                .Include("OrderItems")
                .Where(o => o.CustomerId == currentUser.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Profile/Reviews
        public ActionResult Reviews()
        {
            var currentUser = CurrentUser;
            var reviews = Db.Reviews
                .Include("Book")
                .Where(r => r.CustomerId == currentUser.UserId)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            return View(reviews);
        }

        // GET: Profile/SellerDashboard
        [SellerOnly]
        public ActionResult SellerDashboard()
        {
            var currentUser = CurrentUser;
            
            // Get seller's books
            var sellerBooks = Db.Books
                .Where(b => b.SellerId == currentUser.UserId)
                .ToList();

            // Get orders for seller's books
            var sellerOrders = Db.OrderItems
                .Include("Order")
                .Include("Book")
                .Where(oi => oi.Book.SellerId == currentUser.UserId)
                .GroupBy(oi => oi.OrderId)
                .Select(g => g.FirstOrDefault().Order)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();

            var model = new SellerDashboardViewModel
            {
                TotalBooks = sellerBooks.Count,
                TotalOrders = sellerOrders.Count,
                TotalRevenue = sellerOrders.Sum(o => o.TotalAmount),
                LowStockBooks = sellerBooks.Where(b => b.StockQuantity < 5).ToList(),
                RecentOrders = sellerOrders,
                BooksSold = Db.OrderItems
                    .Where(oi => oi.Book.SellerId == currentUser.UserId)
                    .Sum(oi => (int?)oi.Quantity) ?? 0
            };

            return View(model);
        }

        // GET: Profile/AdminDashboard
        [AdminOnly]
        public ActionResult AdminDashboard()
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            
            var model = new AdminDashboardViewModel
            {
                TotalUsers = Db.Users.Count(),
                TotalBooks = Db.Books.Count(),
                TotalOrders = Db.Orders.Count(),
                TotalRevenue = Db.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered.ToString()).Sum(o => (decimal?)o.TotalAmount) ?? 0,
                PendingOrders = Db.Orders.Count(o => o.OrderStatus == OrderStatus.Pending.ToString()),
                RegisteredUsersThisMonth = Db.Users.Count(u => u.CreatedDate >= oneMonthAgo),
                RecentUsers = Db.Users.OrderByDescending(u => u.CreatedDate).Take(10).ToList(),
                RecentOrders = Db.Orders.Include("Customer").OrderByDescending(o => o.OrderDate).Take(10).ToList(),
                TopSellingBooks = Db.OrderItems
                    .GroupBy(oi => oi.BookId)
                    .Select(g => new { BookId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(10)
                    .Join(Db.Books, x => x.BookId, b => b.BookId, (x, b) => b)
                    .ToList()
            };

            return View(model);
        }

        // GET: Profile/EmployeeDashboard
        [EmployeeOnly]
        public ActionResult EmployeeDashboard()
        {
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            
            var model = new EmployeeDashboardViewModel
            {
                PendingOrders = Db.Orders.Count(o => o.OrderStatus == OrderStatus.Pending.ToString()),
                ShippedOrders = Db.Orders.Count(o => o.OrderStatus == OrderStatus.Shipped.ToString()),
                TotalRevenueThisMonth = Db.Orders
                    .Where(o => o.OrderDate >= oneMonthAgo && o.OrderStatus == OrderStatus.Delivered.ToString())
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,
                RecentOrders = Db.Orders
                    .Include("Customer")
                    .OrderByDescending(o => o.OrderDate)
                    .Take(15)
                    .ToList(),
                LowStockBooks = Db.Books.Where(b => b.StockQuantity < 10).ToList(),
                RecentReviews = Db.Reviews
                    .OrderByDescending(r => r.ReviewDate)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }

        // GET: Profile/MyBooks
        [SellerOnly]
        public ActionResult MyBooks(string search = "", string status = "", string sort = "title", int page = 1)
        {
            var currentUser = CurrentUser;
            var query = Db.Books.Where(b => b.SellerId == currentUser.UserId);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "instock":
                        query = query.Where(b => b.StockQuantity > 10);
                        break;
                    case "lowstock":
                        query = query.Where(b => b.StockQuantity > 0 && b.StockQuantity <= 10);
                        break;
                    case "outofstock":
                        query = query.Where(b => b.StockQuantity <= 0);
                        break;
                }
            }

            // Apply sorting
            switch (sort)
            {
                case "author":
                    query = query.OrderBy(b => b.Author);
                    break;
                case "stock":
                    query = query.OrderBy(b => b.StockQuantity);
                    break;
                case "price":
                    query = query.OrderBy(b => b.Price);
                    break;
                case "date":
                    query = query.OrderByDescending(b => b.CreatedDate);
                    break;
                default: // title
                    query = query.OrderBy(b => b.Title);
                    break;
            }

            var books = query.ToList();

            // Calculate statistics
            var totalBooks = books.Count;
            var booksInStock = books.Count(b => b.StockQuantity > 10);
            var lowStockBooks = books.Count(b => b.StockQuantity > 0 && b.StockQuantity <= 10);
            var outOfStockBooks = books.Count(b => b.StockQuantity <= 0);

            // Pagination
            int pageSize = 10;
            int totalPages = (int)Math.Ceiling((double)books.Count / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));
            var pagedBooks = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new MyBooksViewModel
            {
                Books = pagedBooks,
                TotalBooks = totalBooks,
                BooksInStock = booksInStock,
                LowStockBooks = lowStockBooks,
                OutOfStockBooks = outOfStockBooks,
                SearchTerm = search,
                StatusFilter = status,
                SortBy = sort,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Profile/EditBook/5
        [SellerOnly]
        public ActionResult EditBook(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("MyBooks");
            }

            var currentUser = CurrentUser;
            var book = Db.Books
                .FirstOrDefault(b => b.BookId == id && b.SellerId == currentUser.UserId);

            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
        }

        // POST: Profile/EditBook/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SellerOnly]
        public ActionResult EditBook(Book model, HttpPostedFileBase coverImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = CurrentUser;
            var book = Db.Books
                .FirstOrDefault(b => b.BookId == model.BookId && b.SellerId == currentUser.UserId);

            if (book == null)
            {
                return HttpNotFound();
            }

            // Handle image upload
            if (coverImageFile != null && coverImageFile.ContentLength > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = System.IO.Path.GetExtension(coverImageFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("coverImageFile", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                    return View(model);
                }

                // Validate file size (max 2MB)
                if (coverImageFile.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("coverImageFile", "Image size must be less than 2MB.");
                    return View(model);
                }

                try
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsDir = Server.MapPath("~/Uploads/Books/");
                    if (!System.IO.Directory.Exists(uploadsDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadsDir);
                    }

                    // Generate unique filename
                    var fileName = $"book_{model.BookId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var filePath = System.IO.Path.Combine(uploadsDir, fileName);

                    // Save the file
                    coverImageFile.SaveAs(filePath);

                    // Update the CoverImageUrl
                    model.CoverImageUrl = $"/Uploads/Books/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("coverImageFile", "Error uploading image: " + ex.Message);
                    return View(model);
                }
            }

            // Update book details
            book.Title = model.Title;
            book.Author = model.Author;
            book.ISBN = model.ISBN;
            book.PublicationYear = model.PublicationYear;
            book.Description = model.Description;
            book.Price = model.Price;
            book.StockQuantity = model.StockQuantity;
            book.Category = model.Category;
            book.Genre = model.Genre;
            book.CoverImageUrl = model.CoverImageUrl;
            book.IsAvailable = model.IsAvailable;
            book.LastUpdatedDate = DateTime.Now;

            Db.SaveChanges();

            TempData["Success"] = "Book updated successfully!";
            return RedirectToAction("MyBooks");
        }

        // GET: Profile/ManageUsers
        [AdminOnly]
        public ActionResult ManageUsers()
        {
            var users = Db.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            return View(users);
        }

        // POST: Profile/UpdateUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public ActionResult UpdateUserRole(int userId, UserRole newRole)
        {
            var user = Db.Users.Find(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            user.Role = newRole.ToString();

            Db.SaveChanges();

            return Json(new { success = true, message = "User role updated successfully." });
        }

        // POST: Profile/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public ActionResult ToggleUserStatus(int userId)
        {
            var user = Db.Users.Find(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            user.IsActive = !user.IsActive;

            Db.SaveChanges();

            return Json(new { success = true, message = "User status updated successfully." });
        }

        // GET: Profile/Dashboard
        public ActionResult Dashboard()
        {
            var currentUser = CurrentUser;
            
            // Redirect to role-specific dashboard
            switch (currentUser.Role)
            {
                case "Admin":
                    return RedirectToAction("AdminDashboard");
                case "Seller":
                    return RedirectToAction("SellerDashboard");
                case "Employee":
                    return RedirectToAction("EmployeeDashboard");
                default:
                    // For regular users, redirect to profile index
                    return RedirectToAction("Index");
            }
        }

        // GET: Profile/CreateUser
        [AdminOnly]
        public ActionResult CreateUser()
        {
            var model = new CreateUserViewModel
            {
                AvailableRoles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Employee", Text = "Employee - Manage bookstore operations" },
                    new SelectListItem { Value = "Admin", Text = "Admin - Full system access" }
                }
            };
            return View(model);
        }

        // POST: Profile/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public ActionResult CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate a temporary password
                    var tempPassword = GenerateTemporaryPassword();
                    
                    // Create the user
                    var user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Username = model.Username,
                        PasswordHash = WebApplication1.Helpers.PasswordHelper.HashPassword(tempPassword),
                        Role = model.Role,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        City = model.City,
                        Province = model.Province,
                        PostalCode = model.PostalCode,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    Db.Users.Add(user);
                    Db.SaveChanges();

                    // In a real application, you would send an email with the temporary password
                    // For now, we'll store it in TempData to display to the admin
                    TempData["NewUserPassword"] = tempPassword;
                    TempData["SuccessMessage"] = $"User {model.FirstName} {model.LastName} created successfully!";
                    
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating user: " + ex.Message);
                }
            }

            // If we got this far, something failed; redisplay form
            model.AvailableRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Employee", Text = "Employee - Manage bookstore operations" },
                new SelectListItem { Value = "Admin", Text = "Admin - Full system access" }
            };
            return View(model);
        }

        private string GenerateTemporaryPassword()
        {
            // Generate a random 12-character password
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}