using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Services
{
    public class OrderService
    {
        private readonly BookstoreDbContext _db;

        public OrderService(BookstoreDbContext db)
        {
            _db = db;
        }

        public Order CreateOrderFromCart(int customerId, CheckoutViewModel checkoutModel)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // Get cart items
                    var cartItems = _db.CartItems
                        .Include(ci => ci.Book)
                        .Where(ci => ci.CustomerId == customerId)
                        .ToList();

                    if (!cartItems.Any())
                    {
                        throw new InvalidOperationException("Cart is empty");
                    }

                    // Validate stock availability
                    foreach (var item in cartItems)
                    {
                        if (item.Book.StockQuantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"Insufficient stock for {item.Book.Title}");
                        }
                    }

                    // Create order
                    var order = new Order
                    {
                        OrderNumber = Helpers.PasswordHelper.GenerateOrderNumber(),
                        CustomerId = customerId,
                        OrderDate = DateTime.Now,
                        TotalAmount = checkoutModel.TotalAmount,
                        OrderStatus = OrderStatus.Pending,
                        PaymentStatus = PaymentStatus.Pending,
                        ShippingAddress = checkoutModel.ShippingAddress,
                        ShippingCity = checkoutModel.ShippingCity,
                        ShippingProvince = checkoutModel.ShippingProvince,
                        ShippingPostalCode = checkoutModel.ShippingPostalCode,
                        PaymentMethod = checkoutModel.PaymentMethod,
                        Notes = checkoutModel.Notes
                    };

                    _db.Orders.Add(order);
                    _db.SaveChanges();

                    // Create order items and update stock
                    foreach (var cartItem in cartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            BookId = cartItem.BookId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.Book.Price,
                            Subtotal = cartItem.Quantity * cartItem.Book.Price
                        };

                        _db.OrderItems.Add(orderItem);

                        // Update book stock
                        cartItem.Book.StockQuantity -= cartItem.Quantity;
                    }

                    // Remove cart items
                    _db.CartItems.RemoveRange(cartItems);
                    
                    _db.SaveChanges();
                    transaction.Commit();

                    return order;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public Order GetOrderDetails(int orderId, int? userId = null, UserRole? userRole = null)
        {
            var query = _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.OrderId == orderId);

            // Apply role-based filtering
            if (userId.HasValue && userRole.HasValue)
            {
                switch (userRole.Value)
                {
                    case UserRole.Customer:
                        query = query.Where(o => o.CustomerId == userId.Value);
                        break;
                    case UserRole.Seller:
                        var sellerBookIds = _db.Books
                            .Where(b => b.SellerId == userId.Value)
                            .Select(b => b.BookId)
                            .ToList();
                        query = query.Where(o => o.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)));
                        break;
                }
            }

            return query.FirstOrDefault();
        }

        public List<Order> GetOrdersForUser(int userId, UserRole userRole)
        {
            var query = _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            switch (userRole)
            {
                case UserRole.Customer:
                    query = query.Where(o => o.CustomerId == userId);
                    break;
                case UserRole.Seller:
                    var sellerBookIds = _db.Books
                        .Where(b => b.SellerId == userId)
                        .Select(b => b.BookId)
                        .ToList();
                    query = query.Where(o => o.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)));
                    break;
                case UserRole.Admin:
                case UserRole.Employee:
                    // No additional filtering needed for admins and employees
                    break;
            }

            return query.OrderByDescending(o => o.OrderDate).ToList();
        }

        public bool UpdateOrderStatus(int orderId, OrderStatus newStatus, int? userId = null, UserRole? userRole = null)
        {
            var order = _db.Orders.Find(orderId);
            if (order == null) return false;

            // Check permissions
            if (userId.HasValue && userRole.HasValue)
            {
                if (userRole.Value.ToString() == "Customer" && order.CustomerId != userId.Value)
                {
                    return false;
                }
                if (userRole.Value.ToString() == "Seller")
                {
                    var sellerBookIds = _db.Books
                        .Where(b => b.SellerId == userId.Value)
                        .Select(b => b.BookId)
                        .ToList();
                    if (!order.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)))
                    {
                        return false;
                    }
                }
            }

            order.OrderStatus = newStatus.ToString();
            
            // Update payment status if order is delivered
            if (newStatus.ToString() == "Delivered")
            {
                order.PaymentStatus = PaymentStatus.Paid.ToString();
            }

            _db.SaveChanges();
            return true;
        }

        public bool CancelOrder(int orderId, int userId, UserRole userRole)
        {
            var order = _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null) return false;

            // Check permissions
            if (userRole.ToString() == "Customer" && order.CustomerId != userId)
            {
                return false;
            }

            // Only allow cancellation of pending orders
            if (order.OrderStatus != OrderStatus.Pending.ToString())
            {
                return false;
            }

            // Restore book stock
            foreach (var orderItem in order.OrderItems)
            {
                var book = _db.Books.Find(orderItem.BookId);
                if (book != null)
                {
                    book.StockQuantity += orderItem.Quantity;
                }
            }

            order.OrderStatus = OrderStatus.Cancelled.ToString();
            order.PaymentStatus = PaymentStatus.Refunded.ToString();
            _db.SaveChanges();

            return true;
        }

        public OrderStatisticsViewModel GetOrderStatistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _db.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var orders = query.ToList();

            return new OrderStatisticsViewModel
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending.ToString()),
                ShippedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Shipped.ToString()),
                CompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered.ToString()),
                CancelledOrders = orders.Count(o => o.OrderStatus == OrderStatus.Cancelled.ToString()),
                TotalRevenue = orders.Where(o => o.OrderStatus == OrderStatus.Delivered.ToString()).Sum(o => o.TotalAmount),
                PendingRevenue = orders.Where(o => o.OrderStatus == OrderStatus.Pending.ToString()).Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                TotalBooksSold = _db.OrderItems.Where(oi => orders.Select(o => o.OrderId).Contains(oi.OrderId)).Sum(oi => oi.Quantity)
            };
        }

        public List<Order> GetOrdersByStatus(OrderStatus status, int? userId = null, UserRole? userRole = null)
        {
            var query = _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == status.ToString())
                .AsQueryable();

            if (userId.HasValue && userRole.HasValue)
            {
                switch (userRole.Value)
                {
                    case UserRole.Customer:
                        query = query.Where(o => o.CustomerId == userId.Value);
                        break;
                    case UserRole.Seller:
                        var sellerBookIds = _db.Books
                            .Where(b => b.SellerId == userId.Value)
                            .Select(b => b.BookId)
                            .ToList();
                        query = query.Where(o => o.OrderItems.Any(oi => sellerBookIds.Contains(oi.BookId)));
                        break;
                }
            }

            return query.OrderByDescending(o => o.OrderDate).ToList();
        }
    }
}