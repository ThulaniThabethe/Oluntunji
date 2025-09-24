using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            try
            {
                // Get featured books
                var featuredBooks = Db.Books
                    .Where(b => b.IsAvailable && b.StockQuantity > 0)
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(8)
                    .ToList();

                // Get best selling books (based on order items)
                var bestSellers = Db.OrderItems
                    .GroupBy(oi => oi.BookId)
                    .Select(g => new { BookId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(6)
                    .Join(Db.Books, x => x.BookId, b => b.BookId, (x, b) => b)
                    .Where(b => b.IsAvailable && b.StockQuantity > 0)
                    .ToList();

                // Get categories with book counts (using distinct categories from books)
                var categories = Db.Books
                    .Where(b => b.IsAvailable && b.StockQuantity > 0 && !string.IsNullOrEmpty(b.Category))
                    .GroupBy(b => b.Category)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        BookCount = g.Count()
                    })
                    .Where(c => c.BookCount > 0)
                    .OrderByDescending(c => c.BookCount)
                    .Take(8)
                    .ToList();

                ViewBag.FeaturedBooks = featuredBooks;
                ViewBag.BestSellers = bestSellers;
                ViewBag.Categories = categories;
                ViewBag.TotalBooks = Db.Books.Count(b => b.IsAvailable);
                ViewBag.TotalCategories = categories.Count;

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in HomeController.Index: {ex.Message}");
                return View("Error");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "About Connect2us";
            ViewBag.Mission = "To connect people through quality books, supporting local authors and promoting literacy.";
            ViewBag.Vision = "To become the leading online bookstore, known for our extensive collection, excellent service, and commitment to education and culture.";
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Get in touch with Connect2us";
            ViewBag.Address = "123 Book Street, Johannesburg, 2000";
            ViewBag.Phone = "+27 11 123 4567";
            ViewBag.Email = "info@connect2us.co.za";
            ViewBag.Hours = "Monday - Friday: 8:00 AM - 6:00 PM\nSaturday: 9:00 AM - 4:00 PM\nSunday: Closed";
            
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Terms()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            var faqs = new List<dynamic>
            {
                new { Question = "How do I place an order?", Answer = "Simply browse our collection, add books to your cart, and proceed to checkout. You'll need to create an account or login to complete your purchase." },
                new { Question = "What payment methods do you accept?", Answer = "We accept credit cards, debit cards, EFT, and mobile payment options. All payments are processed securely." },
                new { Question = "How long does delivery take?", Answer = "Standard delivery takes 3-5 business days for major cities and 5-7 business days for outlying areas. Express delivery options are available." },
                new { Question = "Can I return a book?", Answer = "Yes, you can return books within 30 days of delivery if they are in their original condition. Please see our returns policy for full details." },
                new { Question = "Do you ship internationally?", Answer = "Currently, we only ship within selected regions. International shipping may be available in the future." },
                new { Question = "How do I track my order?", Answer = "Once your order is shipped, you'll receive a tracking number via email. You can use this to track your package on our website." }
            };
            
            ViewBag.FAQs = faqs;
            return View();
        }

        [HttpPost]
        public ActionResult Contact(string name, string email, string subject, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    return View();
                }

                // Here you would typically send an email or save to database
                // For now, we'll just show a success message
                TempData["Success"] = "Thank you for your message! We'll get back to you within 24 hours.";
                
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while sending your message. Please try again.";
                System.Diagnostics.Debug.WriteLine($"Error in Contact form: {ex.Message}");
                return View();
            }
        }

        // GET: Home/ContactUs (Enhanced Contact Page)
        public ActionResult ContactUs()
        {
            return View();
        }

        // POST: Home/ContactUs (Enhanced Contact Page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs(ContactViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Here you would typically save to database or send email
                    // For now, we'll just show a success message
                    TempData["Success"] = "Thank you for your message! We'll get back to you within 24 hours.";
                    return RedirectToAction("ContactUs");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while sending your message. Please try again.";
                System.Diagnostics.Debug.WriteLine($"Error in ContactUs form: {ex.Message}");
                return View(model);
            }
        }

        // GET: Home/TestContact (Test Contact Page)
        public ActionResult TestContact()
        {
            return Content("<h1>Test Contact Page</h1><p>This is a test page to check if routing works.</p>", "text/html");
        }


    }
}