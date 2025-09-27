using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FinanceController : Controller
    {
        private readonly BookstoreDbContext _db = new BookstoreDbContext();

        // GET: Finance/Withdraw
        public ActionResult Withdraw()
        {
            return View();
        }

        // POST: Finance/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Withdrawal amount must be greater than zero.";
                return View();
            }

            // For the purpose of this mock system, we'll just record the withdrawal.
            // In a real system, you would integrate with a payment gateway to process the withdrawal.

            var user = _db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);

            var withdrawal = new Withdrawal
            {
                UserId = user.UserId,
                Amount = amount,
                WithdrawalDate = DateTime.Now,
                Status = "Completed"
            };

            _db.Withdrawals.Add(withdrawal);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Withdrawal of {amount:C} processed successfully.";

            return RedirectToAction("Withdraw");
        }
    }
}