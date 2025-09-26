using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private BookstoreDbContext Db = new BookstoreDbContext();

        // GET: Review/ManageReviews
        public ActionResult ManageReviews()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Review> reviewsQuery;

            // Admin and Employee can see all reviews
            if (currentUser.Role == "Admin" || currentUser.Role == "Employee")
            {
                reviewsQuery = Db.Reviews.Include(r => r.Book).Include(r => r.Customer);
            }
            // Seller can see reviews for their books
            else if (currentUser.Role == "Seller")
            {
                reviewsQuery = Db.Reviews
                    .Include(r => r.Book)
                    .Include(r => r.Customer)
                    .Where(r => r.Book.SellerId == currentUser.UserId);
            }
            // Buyer can only see their own reviews
            else if (currentUser.Role == "Buyer")
            {
                reviewsQuery = Db.Reviews
                    .Include(r => r.Book)
                    .Where(r => r.CustomerId == currentUser.UserId);
            }
            else
            {
                return new HttpStatusCodeResult(403, "Access denied");
            }

            var reviews = reviewsQuery.OrderByDescending(r => r.ReviewDate).ToList();
            return View(reviews);
        }

        // POST: Review/ApproveReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReview(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Employee" && currentUser.Role != "Seller"))
            {
                return new HttpStatusCodeResult(403, "Access denied");
            }

            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            // Check if seller owns the book being reviewed
            if (currentUser.Role == "Seller")
            {
                var book = Db.Books.Find(review.BookId);
                if (book == null || book.SellerId != currentUser.UserId)
                {
                    return new HttpStatusCodeResult(403, "Access denied");
                }
            }

            review.IsApproved = true;
            Db.Entry(review).State = EntityState.Modified;
            Db.SaveChanges();

            TempData["Success"] = "Review approved successfully.";
            return RedirectToAction("ManageReviews");
        }

        // POST: Review/RejectReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectReview(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Employee" && currentUser.Role != "Seller"))
            {
                return new HttpStatusCodeResult(403, "Access denied");
            }

            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            // Check if seller owns the book being reviewed
            if (currentUser.Role == "Seller")
            {
                var book = Db.Books.Find(review.BookId);
                if (book == null || book.SellerId != currentUser.UserId)
                {
                    return new HttpStatusCodeResult(403, "Access denied");
                }
            }

            review.IsApproved = false;
            Db.Entry(review).State = EntityState.Modified;
            Db.SaveChanges();

            TempData["Success"] = "Review rejected successfully.";
            return RedirectToAction("ManageReviews");
        }

        // POST: Review/DeleteReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReview(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Role != "Employee"))
            {
                return new HttpStatusCodeResult(403, "Access denied");
            }

            var review = Db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }

            Db.Reviews.Remove(review);
            Db.SaveChanges();

            TempData["Success"] = "Review deleted successfully.";
            return RedirectToAction("ManageReviews");
        }

        private User GetCurrentUser()
        {
            var username = User.Identity.Name;
            return Db.Users.FirstOrDefault(u => u.Username == username);
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