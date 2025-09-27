using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PrintingController : Controller
    {
        private readonly BookstoreDbContext _db = new BookstoreDbContext();
        private readonly IEmailService _emailService = new EmailService();
        private readonly NotificationService _notificationService;

        public PrintingController()
        {
            _notificationService = new NotificationService(_db);
        }

        // GET: Printing/Upload
        public ActionResult Upload()
        {
            return View();
        }

        // POST: Printing/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase file, DeliveryOrPickup option)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string folderPath = Server.MapPath("~/UploadedFiles");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filePath = Path.Combine(folderPath, fileName);
                    file.SaveAs(filePath);

                    // Mock page count and cost calculation
                    int pageCount = new Random().Next(10, 101); // Random page count between 10 and 100
                    decimal cost = pageCount * 3.00m; // R3 per page

                    if (option == DeliveryOrPickup.Delivery)
                    {
                        cost += 10.00m; // R10 for delivery
                    }

                    // Store checkout data in TempData for the checkout page
                var checkoutData = new PrintingCheckoutViewModel
                {
                    FileName = fileName,
                    FilePath = filePath,
                    PageCount = pageCount,
                    Cost = cost,
                    Option = option
                };
                
                TempData["CheckoutData"] = checkoutData;

                    return RedirectToAction("Checkout");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "File upload failed: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
            }

            return View();
        }

        // GET: Printing/Checkout
        public ActionResult Checkout()
        {
            var checkoutData = TempData["CheckoutData"] as PrintingCheckoutViewModel;
            if (checkoutData == null)
            {
                return RedirectToAction("Upload");
            }
            
            // Keep the data for the next request (in case user refreshes)
            TempData.Keep("CheckoutData");
            
            return View(checkoutData);
        }

        // POST: Printing/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(string PaymentMethod, string CardNumber, string CardholderName, string CVV, string ExpiryMonth, string ExpiryYear)
        {
            var checkoutData = TempData["CheckoutData"] as PrintingCheckoutViewModel;
            if (checkoutData == null)
            {
                return RedirectToAction("Upload");
            }

            // Get the current user
            var user = _db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Upload");
            }

            // Validate payment information (basic validation)
            if (string.IsNullOrEmpty(PaymentMethod) || string.IsNullOrEmpty(CardNumber) || 
                string.IsNullOrEmpty(CardholderName) || string.IsNullOrEmpty(CVV) || 
                string.IsNullOrEmpty(ExpiryMonth) || string.IsNullOrEmpty(ExpiryYear))
            {
                TempData["ErrorMessage"] = "Please fill in all payment information.";
                TempData["CheckoutData"] = checkoutData; // Keep checkout data
                return RedirectToAction("Checkout");
            }

            // Simulate payment processing (in real app, integrate with payment gateway)
            bool paymentSuccessful = true; // Mock successful payment

            if (!paymentSuccessful)
            {
                TempData["ErrorMessage"] = "Payment processing failed. Please try again.";
                TempData["CheckoutData"] = checkoutData; // Keep checkout data
                return RedirectToAction("Checkout");
            }

            // Create the print request with payment information
            var printRequest = new PrintRequest
            {
                UserId = user.UserId,
                FilePath = checkoutData.FilePath,
                PageCount = checkoutData.PageCount,
                Cost = checkoutData.Cost,
                Option = checkoutData.Option,
                Status = PrintRequestStatus.Pending,
                RequestDate = DateTime.Now
            };

            _db.PrintRequests.Add(printRequest);
            _db.SaveChanges();

            // Create notification for the user
            _notificationService.CreateNotificationForUser(
                user.UserId,
                "Print Order Confirmed",
                $"Your print order for {checkoutData.FileName} has been confirmed and payment processed. Order ID: {printRequest.PrintRequestId}",
                NotificationType.Order,
                NotificationPriority.Normal
            );

            return RedirectToAction("OrderConfirmation", new { id = printRequest.PrintRequestId });
        }

        // POST: Printing/ConfirmOrder (kept for backward compatibility but not used)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOrder()
        {
            // Redirect to checkout to ensure payment is processed
            return RedirectToAction("Checkout");
        }

        // GET: Printing/OrderConfirmation
        public ActionResult OrderConfirmation(int id)
        {
            var printRequest = _db.PrintRequests.Include("User").FirstOrDefault(pr => pr.PrintRequestId == id);
            if (printRequest == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("History");
            }

            return View(printRequest);
        }

        // GET: Printing/History
        public ActionResult History()
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            var printRequests = _db.PrintRequests.Where(pr => pr.UserId == user.UserId).ToList();
            return View(printRequests);
        }

        // GET: Printing/Manage
        [Authorize(Roles = "Admin,Employee")]
        public ActionResult Manage()
        {
            var printRequests = _db.PrintRequests.ToList();
            return View(printRequests);
        }

        // POST: Printing/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> UpdateStatus(int printRequestId, PrintRequestStatus? status)
        {
            var printRequest = _db.PrintRequests.Include("User").FirstOrDefault(pr => pr.PrintRequestId == printRequestId);
            if (printRequest != null && status.HasValue)
            {
                var oldStatus = printRequest.Status;
                printRequest.Status = status.Value;
                _db.SaveChanges();
                
                // Send email notification for status update
                try
                {
                    await _emailService.SendPrintingStatusUpdateAsync(printRequest);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the status update
                    System.Diagnostics.Debug.WriteLine($"Failed to send email notification: {ex.Message}");
                }
                
                TempData["SuccessMessage"] = "Print request status updated successfully.";
            }
            else if (printRequest == null)
            {
                TempData["ErrorMessage"] = "Print request not found.";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid status provided.";
            }

            return RedirectToAction("Manage");
        }

        // GET: Printing/Download
        [Authorize(Roles = "Admin,Employee")]
        public ActionResult Download(int printRequestId)
        {
            var printRequest = _db.PrintRequests.Find(printRequestId);
            if (printRequest != null && System.IO.File.Exists(printRequest.FilePath))
            {
                return File(printRequest.FilePath, "application/octet-stream", Path.GetFileName(printRequest.FilePath));
            }

            TempData["ErrorMessage"] = "File not found.";
            return RedirectToAction("Manage");
        }
    }
}