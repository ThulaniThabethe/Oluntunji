using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            try
            {
                // Get featured books (top 6 featured books)
                var featuredBooks = Db.Books
                    .Where(b => b.IsAvailable && b.IsFeatured)
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(6)
                    .ToList();

                // Get best sellers (top 6 most ordered books) - handle null OrderItems
                var bestSellers = Db.Books
                    .Where(b => b.IsAvailable)
                    .OrderByDescending(b => b.OrderItems != null ? b.OrderItems.Count : 0)
                    .Take(6)
                    .ToList();

                // Get distinct categories from books with book count
                var categories = Db.Books
                    .Where(b => b.IsAvailable && !string.IsNullOrEmpty(b.Category))
                    .GroupBy(b => b.Category)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        BookCount = g.Count()
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                ViewBag.FeaturedBooks = featuredBooks;
                ViewBag.BestSellers = bestSellers;
                ViewBag.Categories = categories;

                return View();
            }
            catch (Exception ex)
            {
                // Log the error and return a basic view with sample data
                System.Diagnostics.Debug.WriteLine($"Error in Index: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Provide sample data for testing
                ViewBag.FeaturedBooks = new List<Book>();
                ViewBag.BestSellers = new List<Book>();
                ViewBag.Categories = new List<dynamic>();
                
                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page. - Updated for Browse functionality";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // Test action for debugging
        public ActionResult TestBrowse()
        {
            return Content("TestBrowse action is working!");
        }
    }
}