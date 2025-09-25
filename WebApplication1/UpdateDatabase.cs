using System;
using System.Data.Entity.Migrations;
using WebApplication1.Models;
using WebApplication1.Migrations;

namespace WebApplication1
{
    class UpdateDatabase
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting database update...");
            
            try
            {
                // Configure migrations
                var configuration = new Configuration();
                var migrator = new DbMigrator(configuration);
                
                // Get pending migrations
                var pendingMigrations = migrator.GetPendingMigrations();
                Console.WriteLine($"Found {pendingMigrations.Count()} pending migrations");
                
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"  - {migration}");
                }
                
                // Update database
                Console.WriteLine("Updating database...");
                migrator.Update();
                
                Console.WriteLine("Database updated successfully!");
                
                // Test connection
                using (var context = new BookstoreDbContext())
                {
                    var userCount = context.Users.Count();
                    Console.WriteLine($"Database connection test successful. Users in database: {userCount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}