using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        // GET: Order/OrderHistory
        public ActionResult OrderHistory(string orderNumber, DateTime? startDate, DateTime? endDate, 
            string orderStatus, string paymentStatus)
        {
            var currentUser = CurrentUser;
            
            // Base query for user's orders
            var query = Db.Orders
                .Include("OrderItems")
                .Where(o => o.CustomerId == currentUser.UserId);

            // Apply filters
            if (!string.IsNullOrEmpty(orderNumber))
            {
                query = query.Where(o => o.OrderNumber.Contains(orderNumber));
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }

            if (!string.IsNullOrEmpty(orderStatus))
            {
                query = query.Where(o => o.OrderStatus == orderStatus);
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(o => o.PaymentStatus == paymentStatus);
            }

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Calculate statistics
            var totalOrders = orders.Count;
            var totalSpent = orders.Sum(o => o.TotalAmount);
            var lastOrderDate = orders.Any() ? orders.Max(o => o.OrderDate) : DateTime.MinValue;
            
            // Get most purchased category
            var orderIds = orders.Select(o => o.OrderId).ToList();
            var mostPurchasedCategory = Db.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .GroupBy(oi => oi.Book.Category)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var model = new CustomerOrderHistoryViewModel
            {
                Orders = orders.Select(o => new OrderListViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    ItemCount = o.OrderItems.Sum(oi => oi.Quantity)
                }).ToList(),
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                LastOrderDate = lastOrderDate,
                MostPurchasedCategory = mostPurchasedCategory
            };

            return View(model);
        }

        // GET: Order/OrderDetails/5
        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("OrderHistory");
            }

            var currentUser = CurrentUser;
            var order = Db.Orders
                .Include("OrderItems")
                .Include("Customer")
                .Include("OrderItems.Book")
                .FirstOrDefault(o => o.OrderId == id && o.CustomerId == currentUser.UserId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Order/CancelOrder
        [HttpPost]
        public ActionResult CancelOrder(int orderId)
        {
            try
            {
                var currentUser = CurrentUser;
                var order = Db.Orders
                    .Include("OrderItems")
                    .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == currentUser.UserId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Check if order can be cancelled
                if (order.OrderStatus != "Pending" && order.OrderStatus != "Processing")
                {
                    return Json(new { success = false, message = "Order cannot be cancelled in current status." });
                }

                // Restore book stock
                foreach (var orderItem in order.OrderItems)
                {
                    var book = Db.Books.Find(orderItem.BookId);
                    if (book != null)
                    {
                        book.StockQuantity += orderItem.Quantity;
                    }
                }

                // Update order status
                order.OrderStatus = "Cancelled";
                order.PaymentStatus = "Refunded";

                Db.SaveChanges();

                // Create notification
                var notificationController = new NotificationController();
                notificationController.CreateNotification(
                    currentUser.UserId,
                    "Order Cancelled",
                    $"Your order #{order.OrderNumber} has been cancelled and refunded.",
                    "Order",
                    "normal",
                    $"/Order/OrderDetails/{order.OrderId}"
                );

                return Json(new { success = true, message = "Order cancelled successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error cancelling order: " + ex.Message });
            }
        }

        // POST: Order/Reorder
        [HttpPost]
        public ActionResult Reorder(int orderId)
        {
            try
            {
                var currentUser = CurrentUser;
                var order = Db.Orders
                    .Include("OrderItems")
                    .Include("OrderItems.Book")
                    .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == currentUser.UserId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Check if all books are still available
                var unavailableBooks = new System.Collections.Generic.List<string>();
                foreach (var orderItem in order.OrderItems)
                {
                    var book = Db.Books.Find(orderItem.BookId);
                    if (book == null || !book.IsAvailable || book.StockQuantity < orderItem.Quantity)
                    {
                        unavailableBooks.Add(orderItem.Book?.Title ?? "Unknown Book");
                    }
                }

                if (unavailableBooks.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = "Some items are no longer available: " + string.Join(", ", unavailableBooks) 
                    });
                }

                // Add items to cart
                var cartController = new CartController();
                foreach (var orderItem in order.OrderItems)
                {
                    cartController.AddToCart(orderItem.BookId, orderItem.Quantity);
                }

                return Json(new { 
                    success = true, 
                    message = "Items added to cart!", 
                    redirectUrl = Url.Action("Index", "Cart") 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error reordering: " + ex.Message });
            }
        }
    }
}