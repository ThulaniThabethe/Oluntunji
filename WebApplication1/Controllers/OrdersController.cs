using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class OrdersController : BaseController
    {
        // GET: Orders
        public ActionResult Index()
        {
            var currentUser = CurrentUser;
            IQueryable<Order> orders;

            if (currentUser.Role == UserRole.Admin.ToString() || currentUser.Role == UserRole.Employee.ToString())
            {
                // Admins and Employees can see all orders
                orders = Db.Orders
                    .Include("Customer")
                    .Include("OrderItems")
                    .OrderByDescending(o => o.OrderDate);
            }
            else if (currentUser.Role == UserRole.Seller.ToString())
            {
                // Sellers can see orders for their books
                var sellerBookIds = Db.Books
                    .Where(b => b.SellerId == currentUser.UserId)
                    .Select(b => b.BookId)
                    .ToList();

                orders = Db.Orders
                    .Include("Customer")
                    .Include("OrderItems")
                    .Where(o => o.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)))
                    .OrderByDescending(o => o.OrderDate);
            }
            else
            {
                // Customers can see only their orders
                orders = Db.Orders
                    .Include("OrderItems")
                    .Where(o => o.CustomerId == currentUser.UserId)
                    .OrderByDescending(o => o.OrderDate);
            }

            return View(orders.ToList());
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var currentUser = CurrentUser;
            var order = Db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Check if user has permission to view this order
            if (currentUser.Role == UserRole.Customer.ToString() && order.CustomerId != currentUser.UserId)
            {
                return new HttpUnauthorizedResult();
            }

            if (currentUser.Role == UserRole.Seller.ToString())
            {
                var sellerBookIds = Db.Books
                    .Where(b => b.SellerId == currentUser.UserId)
                    .Select(b => b.BookId)
                    .ToList();

                if (!order.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)))
                {
                    return new HttpUnauthorizedResult();
                }
            }

            return View(order);
        }

        // GET: Orders/OrderConfirmation/5
        public ActionResult OrderConfirmation(int? orderId)
        {
            if (orderId == null)
            {
                return RedirectToAction("Index");
            }

            var currentUser = CurrentUser;
            var order = Db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == currentUser.UserId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Orders/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOrEmployee]
        public ActionResult UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = Db.Orders.Find(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            order.OrderStatus = newStatus.ToString();
            
            // Update payment status if order is delivered
            if (newStatus.ToString() == OrderStatus.Delivered.ToString())
            {
                order.PaymentStatus = PaymentStatus.Paid.ToString();
            }

            Db.SaveChanges();

            return Json(new { success = true, message = "Order status updated successfully." });
        }

        // POST: Orders/UpdatePaymentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOrEmployee]
        public ActionResult UpdatePaymentStatus(int orderId, PaymentStatus newStatus)
        {
            var order = Db.Orders.Find(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            order.PaymentStatus = newStatus.ToString();
            Db.SaveChanges();

            return Json(new { success = true, message = "Payment status updated successfully." });
        }

        // GET: Orders/Manage
        [AdminOrEmployee]
        public ActionResult Manage()
        {
            var orders = Db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Orders/PendingOrders
        [AdminOrEmployee]
        public ActionResult PendingOrders()
        {
            var pendingOrders = Db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .Where(o => o.OrderStatus == OrderStatus.Pending.ToString())
                .OrderBy(o => o.OrderDate)
                .ToList();

            return View(pendingOrders);
        }

        // GET: Orders/ShippedOrders
        [AdminOrEmployee]
        public ActionResult ShippedOrders()
        {
            var shippedOrders = Db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .Where(o => o.OrderStatus == OrderStatus.Shipped.ToString())
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(shippedOrders);
        }

        // POST: Orders/CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int orderId)
        {
            var currentUser = CurrentUser;
            var order = Db.Orders
                .Include("OrderItems")
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            // Check if user has permission to cancel this order
            if (currentUser.Role == UserRole.Customer.ToString() && order.CustomerId != currentUser.UserId)
            {
                return Json(new { success = false, message = "You don't have permission to cancel this order." });
            }

            // Only allow cancellation of pending orders
            if (order.OrderStatus != OrderStatus.Pending.ToString())
            {
                return Json(new { success = false, message = "Only pending orders can be cancelled." });
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

            order.OrderStatus = OrderStatus.Cancelled.ToString();
            order.PaymentStatus = PaymentStatus.Refunded.ToString();
            Db.SaveChanges();

            return Json(new { success = true, message = "Order cancelled successfully." });
        }

        // GET: Orders/OrderHistory
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

        // GET: Orders/SellerOrders
        [SellerOnly]
        public ActionResult SellerOrders()
        {
            var currentUser = CurrentUser;
            var sellerBookIds = Db.Books
                .Where(b => b.SellerId == currentUser.UserId)
                .Select(b => b.BookId)
                .ToList();

            var orders = Db.Orders
                .Include("Customer")
                .Include("OrderItems")
                .Where(o => o.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
    }
}