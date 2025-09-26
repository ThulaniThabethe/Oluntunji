using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CartController : BaseController
    {
        // GET: Cart
        public ActionResult Index()
        {
            var currentUser = CurrentUser;
            var cartItems = Db.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == currentUser.UserId)
                .ToList();

            var cartViewModel = new CartViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Book.Price)
            };

            return View(cartViewModel);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int bookId, int quantity = 1)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the return URL in TempData to redirect back after login
                TempData["ReturnUrl"] = $"/Books/Details/{bookId}";
                return RedirectToAction("Login", "Account");
            }

            var currentUser = CurrentUser;
            var book = Db.Books.Find(bookId);

            if (book == null || !book.IsAvailable || book.StockQuantity < quantity)
            {
                TempData["Error"] = "Book is not available or out of stock.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            // Check if item already exists in cart
            var existingCartItem = Db.CartItems
                .FirstOrDefault(ci => ci.CustomerId == currentUser.UserId && ci.BookId == bookId);

            if (existingCartItem != null)
            {
                // Update quantity
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItem
                {
                    CustomerId = currentUser.UserId,
                    BookId = bookId,
                    Quantity = quantity,
                    DateAdded = DateTime.Now
                };
                Db.CartItems.Add(cartItem);
            }

            Db.SaveChanges();
            
            // Show success page with book details
            return View("AddToCartSuccess", book);
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var currentUser = CurrentUser;
            var cartItem = Db.CartItems
                .Include(ci => ci.Book)
                .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.CustomerId == currentUser.UserId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found." });
            }

            if (quantity <= 0)
            {
                // Remove item from cart
                Db.CartItems.Remove(cartItem);
            }
            else
            {
                // Update quantity
                if (cartItem.Book.StockQuantity < quantity)
                {
                    return Json(new { success = false, message = "Not enough stock available." });
                }
                cartItem.Quantity = quantity;
            }

            Db.SaveChanges();

            // Calculate new totals
            var cartItems = Db.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == currentUser.UserId)
                .ToList();

            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Book.Price);
            var itemCount = cartItems.Sum(ci => ci.Quantity);

            return Json(new { success = true, totalAmount = totalAmount, itemCount = itemCount });
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int cartItemId)
        {
            var currentUser = CurrentUser;
            var cartItem = Db.CartItems
                .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.CustomerId == currentUser.UserId);

            if (cartItem != null)
            {
                Db.CartItems.Remove(cartItem);
                Db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            var currentUser = CurrentUser;
            var cartItems = Db.CartItems.Where(ci => ci.CustomerId == currentUser.UserId).ToList();

            Db.CartItems.RemoveRange(cartItems);
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Cart/Checkout
        public ActionResult Checkout()
        {
            var currentUser = CurrentUser;
            var cartItems = Db.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == currentUser.UserId)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            // Check stock availability
            foreach (var item in cartItems)
            {
                if (item.Book.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"Insufficient stock for {item.Book.Title}. Only {item.Book.StockQuantity} available.";
                    return RedirectToAction("Index");
                }
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Book.Price),
                ShippingAddress = currentUser.Address,
                ShippingCity = currentUser.City,
                ShippingProvince = currentUser.Province,
                ShippingPostalCode = currentUser.PostalCode
            };

            return View(checkoutViewModel);
        }

        // POST: Cart/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var currentUser = CurrentUser;
            var cartItems = Db.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == currentUser.UserId)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            // Calculate total amount from cart items
            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Book.Price);

            // Create order
            var order = new Order
            {
                OrderNumber = Helpers.PasswordHelper.GenerateOrderNumber(),
                CustomerId = currentUser.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderStatus = OrderStatus.Pending.ToString(),
                PaymentStatus = PaymentStatus.Pending.ToString(),
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingProvince = model.ShippingProvince,
                ShippingPostalCode = model.ShippingPostalCode,
                PaymentMethod = model.PaymentMethod,
                Notes = model.Notes
            };

            Db.Orders.Add(order);
            Db.SaveChanges();

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

                Db.OrderItems.Add(orderItem);

                // Update book stock
                cartItem.Book.StockQuantity -= cartItem.Quantity;
            }

            // Remove cart items
            Db.CartItems.RemoveRange(cartItems);
            Db.SaveChanges();

            // TODO: Send order confirmation email

            return RedirectToAction("OrderConfirmation", "Orders", new { orderId = order.OrderId });
        }
    }
}