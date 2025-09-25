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
                    // Check if database exists
                    if (context.Database.Exists())
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Database already exists",
                            userCount = context.Users.Count()
                        });
                    }

                    // Create the database
                    context.Database.Create();
                    
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
                        
                        return Json(new
                        {
                            success = true,
                            message = "Database initialized successfully with seed data",
                            userCount = 4,
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
    }
}