using System;
using System.Web.Mvc;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class DeliveryController : Controller
    {
        private readonly OrderService _orderService;

        public DeliveryController()
        {
            _orderService = new OrderService(new BookstoreDbContext());
        }

        // GET: Delivery
        public ActionResult Index()
        {
            var deliveries = _orderService.GetDeliveriesForManagement();
            var statistics = _orderService.GetDeliveryStatistics();
            
            ViewBag.Statistics = statistics;
            return View(deliveries);
        }

        // GET: Delivery/Details/5
        public ActionResult Details(int id)
        {
            var delivery = _orderService.GetDeliveryDetails(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            
            return View(delivery);
        }

        // GET: Delivery/Update/5
        public ActionResult Update(int id)
        {
            var order = _orderService.GetOrderDetails(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new DeliveryUpdateViewModel
            {
                OrderId = order.OrderId,
                TrackingNumber = order.TrackingNumber,
                ShippedDate = order.ShippedDate ?? DateTime.Now,
                DeliveredDate = order.DeliveredDate,
                DeliveryNotes = order.Notes
            };

            return View(model);
        }

        // POST: Delivery/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(DeliveryUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool success = _orderService.UpdateDeliveryStatus(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Delivery status updated successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update delivery status.";
                }
            }

            return View(model);
        }

        // GET: Delivery/Search
        public ActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index");
            }

            var deliveries = _orderService.SearchDeliveries(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SearchResults = true;
            
            return View("Index", deliveries);
        }

        // POST: Delivery/MarkAsShipped
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsShipped(int orderId, string trackingNumber)
        {
            try
            {
                var model = new DeliveryUpdateViewModel
                {
                    OrderId = orderId,
                    TrackingNumber = trackingNumber,
                    ShippedDate = DateTime.Now,
                    MarkAsDelivered = false
                };

                bool success = _orderService.UpdateDeliveryStatus(model);
                if (success)
                {
                    // Also update the order status to Shipped
                _orderService.UpdateOrderStatus(orderId, OrderStatus.Shipped);
                    TempData["SuccessMessage"] = "Order marked as shipped successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to mark order as shipped.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Delivery/MarkAsDelivered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsDelivered(int orderId)
        {
            try
            {
                var order = _orderService.GetOrderDetails(orderId);
                if (order != null)
                {
                    var model = new DeliveryUpdateViewModel
                    {
                        OrderId = orderId,
                        TrackingNumber = order.TrackingNumber,
                        ShippedDate = order.ShippedDate ?? DateTime.Now,
                        DeliveredDate = DateTime.Now,
                        MarkAsDelivered = true
                    };

                    bool success = _orderService.UpdateDeliveryStatus(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Order marked as delivered successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to mark order as delivered.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Delivery/Statistics
        public ActionResult Statistics()
        {
            var statistics = _orderService.GetDeliveryStatistics();
            return View(statistics);
        }
    }
}