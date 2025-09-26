using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Migrations;

namespace WebApplication1.Controllers
{
    public class DatabaseController : Controller
    {
        [HttpGet]
        public ActionResult TestConnection()
        {
            try
            {
                using (var context = new BookstoreDbContext())
                {
                    // Try to open the connection
                    context.Database.Connection.Open();
                    var connectionString = context.Database.Connection.ConnectionString;
                    
                    return Json(new
                    {
                        success = true,
                        message = "Database connection successful",
                        connectionString = connectionString,
                        databaseExists = context.Database.Exists()
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Database connection failed",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckExists()
        {
            try
            {
                using (var context = new BookstoreDbContext())
                {
                    var exists = context.Database.Exists();
                    var appDataPath = Server.MapPath("~/App_Data");
                    var databaseFiles = Directory.GetFiles(appDataPath, "*.mdf");
                    
                    return Json(new
                    {
                        success = true,
                        databaseExists = exists,
                        appDataPath = appDataPath,
                        databaseFiles = databaseFiles,
                        userCount = exists ? context.Users.Count() : 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Database check failed",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult InitializeDatabase()
        {
            try
            {
                using (var context = new BookstoreDbContext())
                {
                    // Check if database exists and has users
                    bool databaseExists = context.Database.Exists();
                    int userCount = context.Users.Count();
                    
                    if (databaseExists && userCount > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Database already exists with users",
                            userCount = userCount
                        });
                    }

                    // Create the database if it doesn't exist
                    if (!databaseExists)
                    {
                        context.Database.Create();
                    }
                    
                    // Initialize with seed data
                    try
                    {
                        // Create admin user
                        var admin = new User
                        {
                            FirstName = "Admin",
                            LastName = "User",
                            Email = "admin@connect2us.co.za",
                            Username = "admin",
                            PasswordHash = WebApplication1.Helpers.PasswordHelper.HashPassword("Admin@123"),
                            Role = "Admin",
                            PhoneNumber = "+27 82 000 0001",
                            Address = "123 Admin Street",
                            City = "Johannesburg",
                            Province = "Gauteng",
                            PostalCode = "2000",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        var employee = new User
                        {
                            FirstName = "John",
                            LastName = "Smith",
                            Email = "employee@connect2us.co.za",
                            Username = "employee",
                            PasswordHash = WebApplication1.Helpers.PasswordHelper.HashPassword("Employee@123"),
                            Role = "Employee",
                            PhoneNumber = "+27 82 000 0002",
                            Address = "456 Employee Avenue",
                            City = "Cape Town",
                            Province = "Western Cape",
                            PostalCode = "8000",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        var seller = new User
                        {
                            FirstName = "Sarah",
                            LastName = "Johnson",
                            Email = "seller@connect2us.co.za",
                            Username = "seller",
                            PasswordHash = WebApplication1.Helpers.PasswordHelper.HashPassword("Seller@123"),
                            Role = "Seller",
                            PhoneNumber = "+27 82 000 0003",
                            Address = "789 Seller Road",
                            City = "Durban",
                            Province = "KwaZulu-Natal",
                            PostalCode = "4000",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        var customer = new User
                        {
                            FirstName = "Michael",
                            LastName = "Brown",
                            Email = "customer@connect2us.co.za",
                            Username = "customer",
                            PasswordHash = WebApplication1.Helpers.PasswordHelper.HashPassword("Customer@123"),
                            Role = "Customer",
                            PhoneNumber = "+27 82 000 0004",
                            Address = "321 Customer Lane",
                            City = "Pretoria",
                            Province = "Gauteng",
                            PostalCode = "0001",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        context.Users.Add(admin);
                        context.Users.Add(employee);
                        context.Users.Add(seller);
                        context.Users.Add(customer);
                        
                        context.SaveChanges();
                        
                        // Add notification columns to Users table
                        try
                        {
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
                        }
                        catch (Exception sqlEx)
                        {
                            // Log the error but don't fail the entire initialization
                            System.Diagnostics.Debug.WriteLine($"Warning: Could not add notification columns: {sqlEx.Message}");
                        }
                        
                        return Json(new
                        {
                            success = true,
                            message = "Database initialized successfully with seed data",
                            userCount = context.Users.Count(),
                            usersCreated = new[] { "admin", "employee", "seller", "customer" }
                        });
                    }
                    catch (Exception seedEx)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Database created but seed data failed",
                            error = seedEx.Message,
                            stackTrace = seedEx.StackTrace
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Database initialization failed",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // GET: Database/Initialize (Original View Method)
        public ActionResult Initialize()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateNotificationsTable()
        {
            try
            {
                using (var context = new BookstoreDbContext())
                {
                    // Check if the Notifications table already exists
                    var tableExists = context.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Notifications'").FirstOrDefault();

                    if (tableExists > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Notifications table already exists",
                            notificationCount = context.Notifications.Count()
                        });
                    }

                    // Read and execute the SQL script
                    var sqlScriptPath = Server.MapPath("~/CreateNotificationsTable.sql");
                    if (!System.IO.File.Exists(sqlScriptPath))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "SQL script file not found"
                        });
                    }

                    var sqlScript = System.IO.File.ReadAllText(sqlScriptPath);
                    context.Database.ExecuteSqlCommand(sqlScript);

                    return Json(new
                    {
                        success = true,
                        message = "Notifications table created successfully"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to create Notifications table",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        
        // GET: Database/Reset
        public ActionResult Reset()
        {
            try
            {
                // Drop and recreate the database
                Database.SetInitializer(new DropCreateDatabaseAlways<BookstoreDbContext>());
                
                using (var context = new BookstoreDbContext())
                {
                    context.Database.Initialize(true);
                }
                
                // Re-seed the data
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookstoreDbContext, Configuration>());
                
                ViewBag.Message = "Database reset successfully!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error resetting database: {ex.Message}";
                ViewBag.Success = false;
            }
            
            return View("Initialize");
        }

        [HttpPost]
        public ActionResult UpdateNotificationColumns()
        {
            try
            {
                using (var context = new BookstoreDbContext())
                {
                    // Execute the SQL script to add notification columns
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
                    
                    return Json(new
                    {
                        success = true,
                        message = "Notification columns added successfully to Users table"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error updating notification columns",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}