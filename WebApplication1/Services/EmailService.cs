using System;
using System.Configuration;
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
        Task<bool> SendRegistrationWelcomeAsync(User user);
        Task<bool> SendLowStockAlertAsync(Book book, User seller);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService()
        {
            _smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "your-email@gmail.com";
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "your-app-password";
            _fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? "noreply@connect2us.co.za";
            _fromName = ConfigurationManager.AppSettings["FromName"] ?? "Connect2us";
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

                return await SendEmailAsync(order.Customer.Email, subject, body);
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

                return await SendEmailAsync(order.Customer.Email, subject, body);
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
                                <p style='margin: 0; color: #856404;'><strong>Security Notice:</strong> This password reset link will expire in 1 hour for security reasons. If you didn't request this password reset, please ignore this email.</p>
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
    }
}