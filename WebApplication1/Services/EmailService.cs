using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailConfirmationAsync(User user, string confirmationToken);
        Task<bool> SendOrderConfirmationAsync(Order order);
        Task<bool> SendOrderStatusUpdateAsync(Order order, string previousStatus);
        Task<bool> SendPasswordResetAsync(User user, string resetToken);
        Task<bool> SendPasswordResetConfirmationAsync(User user);
        Task<bool> SendRegistrationWelcomeAsync(User user);
        Task<bool> SendAdminCreatedAccountWelcomeAsync(User user, string temporaryPassword);
        Task<bool> SendLowStockAlertAsync(Book book, User seller);
        Task<bool> SendPaymentReceiptAsync(Order order, string transactionId);
        Task<bool> SendPaymentConfirmationAsync(Order order);
        Task<bool> SendPaymentConfirmationAsync(int printRequestId, PaymentResult paymentResult);
        Task<bool> SendPrintingStatusUpdateAsync(PrintRequest printRequest);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly NotificationService _notificationService;

        public EmailService()
        {
            _smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "your-email@gmail.com";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "your-app-password";
            _fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? "noreply@connect2us.co.za";
            _fromName = ConfigurationManager.AppSettings["FromName"] ?? "Connect2us";
            _notificationService = new NotificationService(new BookstoreDbContext());
        }

        public async Task<bool> SendEmailConfirmationAsync(User user, string confirmationToken)
        {
            try
            {
                var confirmationUrl = $"https://localhost:44300/Account/ConfirmEmail?userId={user.UserId}&token={confirmationToken}";
                var subject = "Confirm Your Email Address - Connect2us";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Welcome to Connect2us!</h2>
                            <p>Hello {user.FirstName},</p>
                            <p>Thank you for registering with Connect2us. To complete your registration, please confirm your email address by clicking the button below:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{confirmationUrl}' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Confirm Email Address</a>
                            </div>
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #3498db;'>{confirmationUrl}</p>
                            <p>This confirmation link will expire in 24 hours for security reasons.</p>
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>If you didn't create an account with us, please ignore this email.</p>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error sending email confirmation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationAsync(Order order)
        {
            try
            {
                var subject = $"Order Confirmation #{order.OrderNumber} - Connect2us";
                var orderItemsHtml = "";
                
                foreach (var item in order.OrderItems)
                {
                    orderItemsHtml += $@"
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.Book.Title}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>R{item.UnitPrice:F2}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>R{item.Subtotal:F2}</td>
                        </tr>";
                }

                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Order Confirmation</h2>
                            <p>Hello {order.Customer.FirstName},</p>
                            <p>Thank you for your order! We're pleased to confirm that we've received your order and are preparing it for shipment.</p>
                            
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Order Details</h3>
                                <p><strong>Order Number:</strong> #{order.OrderNumber}</p>
                                <p><strong>Order Date:</strong> {order.OrderDate:yyyy-MM-dd HH:mm}</p>
                                <p><strong>Order Status:</strong> {order.OrderStatus}</p>
                                <p><strong>Payment Status:</strong> {order.PaymentStatus}</p>
                            </div>

                            <div style='margin: 20px 0;'>
                                <h3 style='color: #2c3e50;'>Order Items</h3>
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <thead>
                                        <tr style='background-color: #f8f9fa;'>
                                            <th style='padding: 10px; text-align: left; border-bottom: 2px solid #ddd;'>Book Title</th>
                                            <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>Price</th>
                                            <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>Quantity</th>
                                            <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Subtotal</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {orderItemsHtml}
                                    </tbody>
                                    <tfoot>
                                        <tr style='font-weight: bold;'>
                                            <td colspan='3' style='padding: 10px; text-align: right; border-top: 2px solid #ddd;'>Total:</td>
                                            <td style='padding: 10px; text-align: right; border-top: 2px solid #ddd;'>R{order.TotalAmount:F2}</td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </div>

                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Shipping Address</h3>
                                <p>{order.ShippingAddress}</p>
                                <p>{order.ShippingCity}, {order.ShippingProvince} {order.ShippingPostalCode}</p>
                            </div>

                            <p>We'll send you another email when your order has been shipped. If you have any questions about your order, please don't hesitate to contact our customer support.</p>
                            
                            <p>Thank you for choosing Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                var emailSent = await SendEmailAsync(order.Customer.Email, subject, body);
                
                // Create notification for order confirmation
                if (emailSent)
                {
                    _notificationService.CreateNotificationForUser(
                        order.CustomerId,
                        "Order Confirmed",
                        $"Your order #{order.OrderNumber} has been confirmed and is being prepared for shipment.",
                        NotificationType.Order,
                        NotificationPriority.Normal
                    );
                }
                
                return emailSent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending order confirmation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendOrderStatusUpdateAsync(Order order, string previousStatus)
        {
            try
            {
                var subject = $"Order #{order.OrderNumber} Status Update - Connect2us";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Order Status Update</h2>
                            <p>Hello {order.Customer.FirstName},</p>
                            <p>We wanted to let you know that your order status has been updated.</p>
                            
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Order Information</h3>
                                <p><strong>Order Number:</strong> #{order.OrderNumber}</p>
                                <p><strong>Previous Status:</strong> {previousStatus}</p>
                                <p><strong>Current Status:</strong> <span style='color: #28a745; font-weight: bold;'>{order.OrderStatus}</span></p>
                                <p><strong>Payment Status:</strong> {order.PaymentStatus}</p>
                            </div>";

                if (order.OrderStatus == OrderStatus.Shipped.ToString() && !string.IsNullOrEmpty(order.TrackingNumber))
                {
                    body += $@"
                            <div style='background-color: #e8f5e8; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Shipping Information</h3>
                                <p><strong>Tracking Number:</strong> {order.TrackingNumber}</p>
                                <p>You can track your package using the tracking number provided above.</p>
                            </div>";
                }

                body += $@"
                            <p>If you have any questions about your order, please don't hesitate to contact our customer support.</p>
                            <p>Thank you for choosing Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                var emailSent = await SendEmailAsync(order.Customer.Email, subject, body);
                
                // Create notification for order status update
                if (emailSent)
                {
                    _notificationService.CreateNotificationForUser(
                        order.CustomerId,
                        "Order Status Updated",
                        $"Your order #{order.OrderNumber} status has been updated to: {order.OrderStatus}",
                        NotificationType.Order,
                        NotificationPriority.Normal
                    );
                }
                
                return emailSent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending order status update: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(User user, string resetToken)
        {
            try
            {
                var resetUrl = $"https://localhost:44300/Account/ResetPassword?userId={user.UserId}&token={resetToken}";
                var subject = "Password Reset Request - Connect2us";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Password Reset Request</h2>
                            <p>Hello {user.FirstName},</p>
                            <p>We received a request to reset your password for your Connect2us account. If you made this request, please click the button below to reset your password:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetUrl}' style='background-color: #e74c3c; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>
                            </div>
                            
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #e74c3c;'>{resetUrl}</p>
                            
                            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #856404;'><strong>Security Notice:</strong> This password reset link will expire in 1 hour for security reasons. If you didn't request this password reset, please ignore this email and your password will remain unchanged.</p>
                            </div>
                            
                            <p>If you have any questions, please contact our customer support.</p>
                            <p>Thank you for choosing Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending password reset: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetConfirmationAsync(User user)
        {
            try
            {
                var subject = "Password Successfully Changed - Connect2us";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #28a745; text-align: center;'>Password Successfully Changed</h2>
                            <p>Hello {user.FirstName},</p>
                            <p>This email confirms that your password has been successfully changed for your Connect2us account.</p>
                            
                            <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #155724;'><strong>Account Details:</strong></p>
                                <p style='margin: 5px 0; color: #155724;'>Email: {user.Email}</p>
                                <p style='margin: 5px 0; color: #155724;'>Date Changed: {DateTime.Now:yyyy-MM-dd HH:mm} UTC</p>
                            </div>
                            
                            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0; color: #856404;'><strong>Security Notice:</strong> If you did not make this change, please contact our support team immediately and consider securing your account.</p>
                            </div>
                            
                            <p>For your security, we recommend:</p>
                            <ul>
                                <li>Using a strong, unique password</li>
                                <li>Not sharing your password with anyone</li>
                                <li>Logging out of shared devices</li>
                            </ul>
                            
                            <p>If you have any questions or concerns, please contact our customer support.</p>
                            <p>Thank you for choosing Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending password reset confirmation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendRegistrationWelcomeAsync(User user)
        {
            try
            {
                var subject = "Welcome to Connect2us!";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Welcome to Connect2us!</h2>
                            <p>Hello {user.FirstName},</p>
                            <p>Welcome to Connect2us! We're thrilled to have you as part of our community of book lovers.</p>
                            
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Your Account Details</h3>
                                <p><strong>Name:</strong> {user.FirstName} {user.LastName}</p>
                                <p><strong>Email:</strong> {user.Email}</p>
                                <p><strong>Account Type:</strong> {user.Role}</p>
                                <p><strong>Registration Date:</strong> {user.CreatedDate:yyyy-MM-dd}</p>
                            </div>

                            <div style='margin: 20px 0;'>
                                <h3 style='color: #2c3e50;'>What's Next?</h3>
                                <ul style='padding-left: 20px;'>
                                    <li>Explore our vast collection of books across various genres</li>
                                    <li>Add books to your wishlist for future purchases</li>
                                    <li>Leave reviews for books you've read</li>
                                    <li>Track your orders and manage your account</li>
                                </ul>
                            </div>";

                if (user.Role == UserRole.Seller.ToString())
                {
                    body += $@"
                            <div style='background-color: #e8f5e8; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Seller Account</h3>
                                <p>As a seller, you can now:</p>
                                <ul style='padding-left: 20px;'>
                                    <li>List your books for sale</li>
                                    <li>Manage your inventory</li>
                                    <li>Track your sales and revenue</li>
                                    <li>Process customer orders</li>
                                </ul>
                            </div>";
                }

                body += $@"
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='https://localhost:44300/' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Start Shopping</a>
                            </div>
                            
                            <p>If you have any questions or need assistance, please don't hesitate to contact our customer support team.</p>
                            <p>Thank you for joining Connect2us. Happy reading!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending registration welcome: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAdminCreatedAccountWelcomeAsync(User user, string temporaryPassword)
        {
            try
            {
                var subject = "Your Connect2us Account Has Been Created";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Welcome to Connect2us!</h2>
                            <p>Hello {user.FirstName},</p>
                            <p>An administrator has created an account for you on Connect2us. We're excited to have you join our community!</p>
                            
                            <div style='background-color: #e8f5e8; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #155724;'>Your Account Details</h3>
                                <p><strong>Name:</strong> {user.FirstName} {user.LastName}</p>
                                <p><strong>Email:</strong> {user.Email}</p>
                                <p><strong>Username:</strong> {user.Email}</p>
                                <p><strong>Temporary Password:</strong> <span style='background-color: #f8f9fa; padding: 5px; border-radius: 3px; font-family: monospace;'>{temporaryPassword}</span></p>
                                <p><strong>Account Type:</strong> {user.Role}</p>
                            </div>

                            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #856404;'>Important Security Notice</h3>
                                <p><strong>Please change your password immediately after your first login for security reasons.</strong></p>
                                <p>Your temporary password is only meant for initial access and should be changed as soon as possible.</p>
                            </div>

                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='https://localhost:44300/Account/Login' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Login to Your Account</a>
                            </div>

                            <div style='margin: 20px 0;'>
                                <h3 style='color: #2c3e50;'>Getting Started</h3>
                                <ul style='padding-left: 20px;'>
                                    <li>Log in using your email and temporary password</li>
                                    <li>Change your password in your account settings</li>
                                    <li>Complete your profile information</li>
                                    <li>Explore our book collection and services</li>
                                </ul>
                            </div>";

                if (user.Role == UserRole.Seller.ToString())
                {
                    body += $@"
                            <div style='background-color: #cce5ff; border: 1px solid #99ccff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #0066cc;'>Seller Account Features</h3>
                                <p>As a seller, you have access to:</p>
                                <ul style='padding-left: 20px;'>
                                    <li>Book listing and inventory management</li>
                                    <li>Sales tracking and analytics</li>
                                    <li>Order processing and fulfillment</li>
                                    <li>Customer communication tools</li>
                                </ul>
                            </div>";
                }
                else if (user.Role == UserRole.Admin.ToString())
                {
                    body += $@"
                            <div style='background-color: #ffe6e6; border: 1px solid #ffcccc; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #cc0000;'>Administrator Access</h3>
                                <p>As an administrator, you have access to:</p>
                                <ul style='padding-left: 20px;'>
                                    <li>User management and account creation</li>
                                    <li>System configuration and settings</li>
                                    <li>Order and transaction oversight</li>
                                    <li>Platform analytics and reporting</li>
                                </ul>
                            </div>";
                }

                body += $@"
                            <p>If you have any questions or need assistance getting started, please don't hesitate to contact our support team.</p>
                            <p>Welcome to Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending admin created account welcome: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendLowStockAlertAsync(Book book, User seller)
        {
            try
            {
                var subject = $"Low Stock Alert: {book.Title} - Connect2us";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #e74c3c; text-align: center;'>Low Stock Alert</h2>
                            <p>Hello {seller.FirstName},</p>
                            <p>This is to inform you that one of your books is running low on stock.</p>
                            
                            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Book Details</h3>
                                <p><strong>Title:</strong> {book.Title}</p>
                                <p><strong>Author:</strong> {book.Author}</p>
                                <p><strong>ISBN:</strong> {book.ISBN}</p>
                                <p><strong>Current Stock:</strong> <span style='color: #e74c3c; font-weight: bold;'>{book.StockQuantity} units</span></p>
                                <p><strong>Category:</strong> {book.Category}</p>
                            </div>
                            
                            <p style='color: #e74c3c; font-weight: bold;'>We recommend restocking this item soon to avoid running out of stock.</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='https://localhost:44300/Books/Edit/{book.BookId}' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Update Stock</a>
                            </div>
                            
                            <p>If you have any questions, please contact our support team.</p>
                            <p>Thank you for being a valued seller on Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(seller.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending low stock alert: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPaymentReceiptAsync(Order order, string transactionId)
        {
            try
            {
                var subject = $"Payment Receipt #{order.OrderNumber} - Connect2us";
                var orderItemsHtml = "";
                
                foreach (var item in order.OrderItems)
                {
                    orderItemsHtml += $@"
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.Book.Title}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>R{item.UnitPrice:F2}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>R{item.Subtotal:F2}</td>
                        </tr>";
                }

                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #28a745; text-align: center;'>Payment Receipt</h2>
                            <p>Hello {order.Customer.FirstName},</p>
                            <p>Thank you for your payment! This email serves as your official receipt for the transaction.</p>
                            
                            <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #155724;'>Payment Details</h3>
                                <p><strong>Transaction ID:</strong> {transactionId}</p>
                                <p><strong>Order Number:</strong> #{order.OrderNumber}</p>
                                <p><strong>Payment Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>
                                <p><strong>Payment Method:</strong> {order.PaymentMethod}</p>
                                <p><strong>Payment Status:</strong> <span style='color: #28a745; font-weight: bold;'>PAID</span></p>
                            </div>

                            <div style='margin: 20px 0;'>
                                <h3 style='color: #2c3e50;'>Order Items</h3>
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <thead>
                                        <tr style='background-color: #f8f9fa;'>
                                            <th style='padding: 10px; text-align: left; border-bottom: 2px solid #ddd;'>Book Title</th>
                                            <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>Price</th>
                                            <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>Quantity</th>
                                            <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Subtotal</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {orderItemsHtml}
                                    </tbody>
                                    <tfoot>
                                        <tr style='font-weight: bold; background-color: #f8f9fa;'>
                                            <td colspan='3' style='padding: 10px; text-align: right; border-top: 2px solid #ddd;'>Total Amount:</td>
                                            <td style='padding: 10px; text-align: right; border-top: 2px solid #ddd; color: #28a745;'>R{order.TotalAmount:F2}</td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </div>

                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Billing Information</h3>
                                <p><strong>Customer:</strong> {order.Customer.FullName}</p>
                                <p><strong>Email:</strong> {order.Customer.Email}</p>
                                <p><strong>Order Date:</strong> {order.OrderDate:yyyy-MM-dd HH:mm}</p>
                            </div>
                            
                            <p>Please keep this receipt for your records. If you have any questions about this transaction, please contact our customer support.</p>
                            <p>Thank you for choosing Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(order.Customer.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending payment receipt: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPaymentConfirmationAsync(Order order)
        {
            try
            {
                var subject = $"Payment Confirmation #{order.OrderNumber} - Connect2us";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #28a745; text-align: center;'>Payment Confirmed</h2>
                            <p>Hello {order.Customer.FirstName},</p>
                            <p>Great news! Your payment has been successfully processed and confirmed.</p>
                            
                            <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #155724;'>Payment Summary</h3>
                                <p><strong>Order Number:</strong> #{order.OrderNumber}</p>
                                <p><strong>Amount Paid:</strong> R{order.TotalAmount:F2}</p>
                                <p><strong>Payment Method:</strong> {order.PaymentMethod}</p>
                                <p><strong>Payment Date:</strong> {order.PaymentDate?.ToString("yyyy-MM-dd HH:mm") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm")}</p>
                                <p><strong>Transaction Reference:</strong> {order.PaymentReference ?? order.TransactionId}</p>
                            </div>

                            <div style='background-color: #cce5ff; border: 1px solid #99ccff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #0066cc;'>What's Next?</h3>
                                <p>Your order is now being processed and will be prepared for shipment. You'll receive another email with tracking information once your order has been dispatched.</p>
                            </div>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='https://localhost:44300/Orders/Details/{order.OrderId}' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>View Order Details</a>
                            </div>
                            
                            <p>If you have any questions about your payment or order, please don't hesitate to contact our customer support.</p>
                            <p>Thank you for choosing Connect2us!</p>
                            
                            <hr style='border: 1px solid #eee; margin: 30px 0;'>
                            <p style='font-size: 12px; color: #666;'>© 2024 Connect2us. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(order.Customer.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending payment confirmation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPrintingStatusUpdateAsync(PrintRequest printRequest)
        {
            try
            {
                var subject = $"Printing Status Update - Request #{printRequest.PrintRequestId}";
                var statusMessage = GetStatusMessage(printRequest.Status);
                var statusColor = GetStatusColor(printRequest.Status);
                
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #2c3e50; text-align: center;'>Printing Status Update</h2>
                            <p>Hello {printRequest.User.FirstName},</p>
                            <p>{statusMessage}</p>
                            
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #2c3e50;'>Print Request Details</h3>
                                <p><strong>Request ID:</strong> #{printRequest.PrintRequestId}</p>
                                <p><strong>Request Date:</strong> {printRequest.RequestDate:yyyy-MM-dd HH:mm}</p>
                                <p><strong>Status:</strong> <span style='color: {statusColor}; font-weight: bold;'>{printRequest.Status}</span></p>
                                <p><strong>Page Count:</strong> {printRequest.PageCount} pages</p>
                                <p><strong>Total Cost:</strong> {printRequest.Cost:C}</p>
                                <p><strong>Delivery Option:</strong> {printRequest.Option}</p>
                            </div>
                            
                            {GetStatusSpecificContent(printRequest.Status)}
                            
                            <div style='background-color: #e8f4f8; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0;'><strong>Need Help?</strong></p>
                                <p style='margin: 5px 0 0 0;'>Contact our support team at support@connect2us.com or call us at (011) 123-4567.</p>
                            </div>
                            
                            <p style='text-align: center; color: #666; font-size: 12px; margin-top: 30px;'>
                                This is an automated message from Connect2us Printing Services.<br>
                                Please do not reply to this email.
                            </p>
                        </div>
                    </body>
                    </html>";

                var emailSent = await SendEmailAsync(printRequest.User.Email, subject, body);
                
                // Create notification for printing status update
                // Create notification for print status update
                if (emailSent)
                {
                    _notificationService.CreateNotificationForUser(
                        printRequest.UserId,
                        "Print Status Updated",
                        $"Your print request #{printRequest.PrintRequestId} status has been updated to: {printRequest.Status}",
                        NotificationType.Order,
                        NotificationPriority.Normal
                    );
                }
                
                return emailSent;
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error sending printing status update email: {ex.Message}");
                return false;
            }
        }

        private string GetStatusMessage(PrintRequestStatus status)
        {
            switch (status)
            {
                case PrintRequestStatus.Pending:
                    return "We have received your print request and it is currently being reviewed by our team.";
                case PrintRequestStatus.Approved:
                    return "Great news! Your print request has been approved and will be processed shortly.";
                case PrintRequestStatus.Printed:
                    return "Your documents have been successfully printed and are ready for collection or delivery.";
                case PrintRequestStatus.Completed:
                    return "Your print request has been completed successfully. Thank you for using our services!";
                case PrintRequestStatus.Cancelled:
                    return "We regret to inform you that your print request has been cancelled. If you have any questions, please contact our support team.";
                default:
                    return "Your print request status has been updated.";
            }
        }

        private string GetStatusColor(PrintRequestStatus status)
        {
            switch (status)
            {
                case PrintRequestStatus.Pending:
                    return "#ffc107"; // Yellow
                case PrintRequestStatus.Approved:
                    return "#17a2b8"; // Blue
                case PrintRequestStatus.Printed:
                    return "#28a745"; // Green
                case PrintRequestStatus.Completed:
                    return "#28a745"; // Green
                case PrintRequestStatus.Cancelled:
                    return "#dc3545"; // Red
                default:
                    return "#6c757d"; // Gray
            }
        }

        private string GetStatusSpecificContent(PrintRequestStatus status)
        {
            switch (status)
            {
                case PrintRequestStatus.Pending:
                    return @"
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <p style='margin: 0;'><strong>What's Next?</strong></p>
                            <p style='margin: 5px 0 0 0;'>Our team will review your request within 24 hours and notify you once it's approved.</p>
                        </div>";
                case PrintRequestStatus.Approved:
                    return @"
                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #17a2b8;'>
                            <p style='margin: 0;'><strong>What's Next?</strong></p>
                            <p style='margin: 5px 0 0 0;'>Your documents will be printed within the next 2-4 hours. We'll notify you once they're ready.</p>
                        </div>";
                case PrintRequestStatus.Printed:
                    return @"
                        <div style='background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;'>
                            <p style='margin: 0;'><strong>Ready for Collection/Delivery!</strong></p>
                            <p style='margin: 5px 0 0 0;'>Your documents are ready. Please collect them from our office or wait for delivery if you selected that option.</p>
                        </div>";
                case PrintRequestStatus.Completed:
                    return @"
                        <div style='background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;'>
                            <p style='margin: 0;'><strong>Thank You!</strong></p>
                            <p style='margin: 5px 0 0 0;'>We hope you're satisfied with our service. Please consider leaving us a review!</p>
                        </div>";
                case PrintRequestStatus.Cancelled:
                    return @"
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #dc3545;'>
                            <p style='margin: 0;'><strong>Cancellation Reason</strong></p>
                            <p style='margin: 5px 0 0 0;'>If you believe this cancellation was made in error, please contact our support team immediately.</p>
                        </div>";
                default:
                    return "";
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, _fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPaymentConfirmationAsync(int printRequestId, PaymentResult paymentResult)
        {
            try
            {
                // Get print request details
                using (var context = new BookstoreDbContext())
                {
                    var printRequest = context.PrintRequests.Include("User").Where(pr => pr.PrintRequestId == printRequestId).FirstOrDefault();
                    if (printRequest == null) return false;

                    var subject = $"Payment Confirmation - Print Request #{printRequestId}";
                    var body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                                <h2 style='color: #28a745; text-align: center;'>Payment Confirmation</h2>
                                <p>Hello {printRequest.User.FirstName},</p>
                                <p>Thank you for your payment! We have successfully received your payment for the printing service.</p>
                                
                                <div style='background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;'>
                                    <h3 style='margin-top: 0; color: #155724;'>Payment Details</h3>
                                    <p><strong>Transaction ID:</strong> {paymentResult.TransactionId}</p>
                                    <p><strong>Payment Reference:</strong> {paymentResult.PaymentReference}</p>
                                    <p><strong>Amount Paid:</strong> {paymentResult.Amount:C}</p>
                                    <p><strong>Payment Date:</strong> {paymentResult.ProcessedAt:yyyy-MM-dd HH:mm}</p>
                                    <p><strong>Status:</strong> <span style='color: #28a745; font-weight: bold;'>Successful</span></p>
                                </div>
                                
                                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0; color: #2c3e50;'>Print Request Details</h3>
                                    <p><strong>Request ID:</strong> #{printRequest.PrintRequestId}</p>
                                    <p><strong>Page Count:</strong> {printRequest.PageCount} pages</p>
                                    <p><strong>Delivery Option:</strong> {printRequest.Option}</p>
                                    <p><strong>Status:</strong> {printRequest.Status}</p>
                                </div>
                                
                                <div style='background-color: #e8f4f8; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <p style='margin: 0;'><strong>What's Next?</strong></p>
                                    <p style='margin: 5px 0 0 0;'>Your print request will be processed shortly. You'll receive another email when your documents are ready for collection or delivery.</p>
                                </div>
                                
                                <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <p style='margin: 0;'><strong>Need Help?</strong></p>
                                    <p style='margin: 5px 0 0 0;'>Contact our support team at support@connect2us.com or call us at (011) 123-4567.</p>
                                </div>
                                
                                <p style='text-align: center; color: #666; font-size: 12px; margin-top: 30px;'>
                                    This is an automated message from Connect2us Printing Services.<br>
                                    Please keep this email as your payment receipt.
                                </p>
                            </div>
                        </body>
                        </html>";

                    var emailSent = await SendEmailAsync(printRequest.User.Email, subject, body);
                    
                    // Create notification for payment confirmation
                    if (emailSent)
                    {
                        _notificationService.CreateNotificationForUser(
                            printRequest.UserId,
                            "Payment Confirmed",
                            $"Payment for print request #{printRequestId} has been successfully processed.",
                            NotificationType.Payment,
                            NotificationPriority.Normal
                        );
                    }
                    
                    return emailSent;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending payment confirmation email: {ex.Message}");
                return false;
            }
        }
    }
}