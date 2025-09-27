using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PrintingController : Controller
    {
        private readonly BookstoreDbContext _db = new BookstoreDbContext();

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

                    var user = _db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);

                    var printRequest = new PrintRequest
                    {
                        UserId = user.UserId,
                        FilePath = filePath,
                        PageCount = pageCount,
                        Cost = cost,
                        RequestDate = DateTime.Now,
                        Status = PrintRequestStatus.Pending,
                        Option = option
                    };

                    _db.PrintRequests.Add(printRequest);
                    _db.SaveChanges();

                    TempData["SuccessMessage"] = "File uploaded successfully. Your print request has been submitted.";
                    return RedirectToAction("History");
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
        public ActionResult UpdateStatus(int printRequestId, PrintRequestStatus? status)
        {
            var printRequest = _db.PrintRequests.Find(printRequestId);
            if (printRequest != null && status.HasValue)
            {
                printRequest.Status = status.Value;
                _db.SaveChanges();
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