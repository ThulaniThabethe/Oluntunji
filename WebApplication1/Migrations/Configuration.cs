using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.Helpers;

namespace WebApplication1.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication1.Models.BookstoreDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true; // Allow data loss for schema updates
            ContextKey = "WebApplication1.Models.BookstoreDbContext";
        }

        protected override void Seed(WebApplication1.Models.BookstoreDbContext context)
        {
            // Update database schema to add notification columns
            UpdateNotificationColumns(context);
            
            // Seed Users
            SeedUsers(context);
            
            // Seed Books
            SeedBooks(context);
            
            // Seed Sample Data
            SeedSampleOrders(context);
            
            context.SaveChanges();
        }

        private void SeedUsers(BookstoreDbContext context)
        {
            if (!context.Users.Any())
            {
                // Admin user
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@connect2us.co.za",
                    Username = "admin",
                    PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                    Role = "Admin",
                    PhoneNumber = "+27 82 000 0001",
                    Address = "123 Admin Street",
                    City = "Johannesburg",
                    Province = "Gauteng",
                    PostalCode = "2000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    PushNotifications = true
                };

                // Employee user
                var employee = new User
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "employee@connect2us.co.za",
                    Username = "employee",
                    PasswordHash = PasswordHelper.HashPassword("Employee@123"),
                    Role = "Employee",
                    PhoneNumber = "+27 82 000 0002",
                    Address = "456 Employee Avenue",
                    City = "Cape Town",
                    Province = "Western Cape",
                    PostalCode = "8000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    PushNotifications = true
                };

                // Seller user
                var seller = new User
                {
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "seller@connect2us.co.za",
                    Username = "seller",
                    PasswordHash = PasswordHelper.HashPassword("Seller@123"),
                    Role = "Seller",
                    PhoneNumber = "+27 82 000 0003",
                    Address = "789 Seller Road",
                    City = "Durban",
                    Province = "KwaZulu-Natal",
                    PostalCode = "4000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    PushNotifications = true
                };

                // Customer users
                var customer1 = new User
                {
                    FirstName = "Michael",
                    LastName = "Brown",
                    Email = "customer@connect2us.co.za",
                    Username = "customer",
                    PasswordHash = PasswordHelper.HashPassword("Customer@123"),
                    Role = "Customer",
                    PhoneNumber = "+27 82 000 0004",
                    Address = "321 Customer Lane",
                    City = "Pretoria",
                    Province = "Gauteng",
                    PostalCode = "0001",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    PushNotifications = true
                };

                var customer2 = new User
                {
                    FirstName = "Emma",
                    LastName = "Davis",
                    Email = "emma.davis@email.co.za",
                    Username = "emmavis",
                    PasswordHash = PasswordHelper.HashPassword("Emma@123"),
                    Role = "Customer",
                    PhoneNumber = "+27 82 000 0005",
                    Address = "654 User Street",
                    City = "Port Elizabeth",
                    Province = "Eastern Cape",
                    PostalCode = "6000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    PushNotifications = true
                };

                context.Users.AddRange(new[] { admin, employee, seller, customer1, customer2 });
            }
        }

        private void SeedBooks(BookstoreDbContext context)
        {
            if (!context.Books.Any())
            {
                var seller = context.Users.FirstOrDefault(u => u.Role == "Seller");
                if (seller != null)
                {
                    var books = new[]
                    {
                        new Book
                        {
                            Title = "Long Walk to Freedom",
                            Author = "Nelson Mandela",
                            ISBN = "978-0-316-87496-0",
                            Description = "The autobiography of Nelson Mandela, detailing his early life, education, and 27 years in prison.",
                            Price = 299.99m,
                            Category = "Biography",
                            PublicationYear = 1994,
                            Publisher = "Little, Brown and Company",
                            StockQuantity = 50,
                            CoverImageUrl = "/Content/Images/long-walk-to-freedom.jpg",
                            SellerId = seller.UserId,
                            IsAvailable = true,
                            CreatedDate = DateTime.Now
                        },
                        new Book
                        {
                            Title = "Cry, the Beloved Country",
                            Author = "Alan Paton",
                            ISBN = "978-0-743-29877-3",
                            Description = "A novel set in South Africa that explores the racial tensions and social injustices of apartheid.",
                            Price = 189.99m,
                            Category = "Fiction",
                            PublicationYear = 1948,
                            Publisher = "Scribner",
                            StockQuantity = 75,
                            CoverImageUrl = "/Content/Images/cry-the-beloved-country.jpg",
                            SellerId = seller.UserId,
                            IsAvailable = true,
                            CreatedDate = DateTime.Now
                        },
                        new Book
                        {
                            Title = "The Power of One",
                            Author = "Bryce Courtenay",
                            ISBN = "978-0-345-41605-4",
                            Description = "A coming-of-age story set in South Africa during the 1930s and 1940s.",
                            Price = 249.99m,
                            Category = "Fiction",
                            PublicationYear = 1989,
                            Publisher = "Ballantine Books",
                            StockQuantity = 30,
                            CoverImageUrl = "/Content/Images/power-of-one.jpg",
                            SellerId = seller.UserId,
                            IsAvailable = true,
                            CreatedDate = DateTime.Now
                        },
                        new Book
                        {
                            Title = "Disgrace",
                            Author = "J.M. Coetzee",
                            ISBN = "978-0-099-28582-2",
                            Description = "A novel about a South African professor who loses his job and moves to the countryside.",
                            Price = 219.99m,
                            Category = "Fiction",
                            PublicationYear = 1999,
                            Publisher = "Vintage",
                            StockQuantity = 40,
                            CoverImageUrl = "/Content/Images/disgrace.jpg",
                            SellerId = seller.UserId,
                            IsAvailable = true,
                            CreatedDate = DateTime.Now
                        },
                        new Book
                        {
                            Title = "Born a Crime: Stories from a South African Childhood",
                            Author = "Trevor Noah",
                            ISBN = "978-0-399-58819-8",
                            Description = "Comedian Trevor Noah's memoir about growing up in South Africa during apartheid.",
                            Price = 279.99m,
                            Category = "Biography",
                            PublicationYear = 2016,
                            Publisher = "Spiegel & Grau",
                            StockQuantity = 60,
                            CoverImageUrl = "/Content/Images/born-a-crime.jpg",
                            SellerId = seller.UserId,
                            IsAvailable = true,
                            CreatedDate = DateTime.Now
                        }
                    };

                    context.Books.AddRange(books);
                }
            }
        }

        private void SeedSampleOrders(BookstoreDbContext context)
        {
            if (!context.Orders.Any())
            {
                var customer = context.Users.FirstOrDefault(u => u.Username == "customer");
                var books = context.Books.Take(3).ToList();
                
                if (customer != null && books.Any())
                {
                     var order = new Order
                     {
                         CustomerId = customer.UserId,
                         OrderNumber = "ORD-001-2024",
                         OrderDate = DateTime.Now.AddDays(-7),
                         TotalAmount = books.Sum(b => b.Price),
                         OrderStatus = "Delivered",
                         PaymentStatus = "Paid",
                         ShippingAddress = customer.Address,
                         ShippingCity = customer.City,
                         ShippingProvince = customer.Province,
                         ShippingPostalCode = customer.PostalCode,
                         PaymentMethod = "Credit Card"
                     };

                     context.Orders.Add(order);
                     context.SaveChanges();
                     // Add order items
                     foreach (var book in books)
                     {
                         var orderItem = new OrderItem
                         {
                             OrderId = order.OrderId,
                             BookId = book.BookId,
                             Quantity = 1,
                             UnitPrice = book.Price,
                             Subtotal = book.Price
                         };
                         context.OrderItems.Add(orderItem);
                     }
                }
            }
        }

        private void UpdateNotificationColumns(BookstoreDbContext context)
        {
            try
            {
                // Check if the columns exist and add them if they don't
                context.Database.ExecuteSqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'OrderUpdates')
                        ALTER TABLE [dbo].[Users] ADD [OrderUpdates] BIT NOT NULL DEFAULT 1
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'BookAlerts')
                        ALTER TABLE [dbo].[Users] ADD [BookAlerts] BIT NOT NULL DEFAULT 1
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'AccountUpdates')
                        ALTER TABLE [dbo].[Users] ADD [AccountUpdates] BIT NOT NULL DEFAULT 1
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'MarketingEmails')
                        ALTER TABLE [dbo].[Users] ADD [MarketingEmails] BIT NOT NULL DEFAULT 0
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'LowStockAlerts')
                        ALTER TABLE [dbo].[Users] ADD [LowStockAlerts] BIT NOT NULL DEFAULT 1
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PriceDropAlerts')
                        ALTER TABLE [dbo].[Users] ADD [PriceDropAlerts] BIT NOT NULL DEFAULT 1
                ");
                
                System.Diagnostics.Debug.WriteLine("Notification columns added successfully to Users table.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating notification columns: {ex.Message}");
                // Continue with seeding even if column update fails
            }
        }
    }
}