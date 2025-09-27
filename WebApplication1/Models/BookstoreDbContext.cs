using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class BookstoreDbContext : DbContext
    {
        public BookstoreDbContext() : base("name=BookstoreConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Revenue> Revenues { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PrintRequest> PrintRequests { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<SavedCard> SavedCards { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Prevent cascade delete for Seller-Book relationship
            modelBuilder.Entity<Book>()
                .HasRequired(b => b.Seller)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.SellerId)
                .WillCascadeOnDelete(false);

            // Configure Order-Customer relationship
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .WillCascadeOnDelete(false);

            // Configure CartItem relationships
            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Customer)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Book)
                .WithMany(b => b.CartItems)
                .HasForeignKey(ci => ci.BookId)
                .WillCascadeOnDelete(false);

            // Configure Review relationships
            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId)
                .WillCascadeOnDelete(false);

            // Configure OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Book)
                .WithMany(b => b.OrderItems)
                .HasForeignKey(oi => oi.BookId)
                .WillCascadeOnDelete(false);

            // Configure Revenue relationships
            modelBuilder.Entity<Revenue>()
                .HasOptional(r => r.Seller)
                .WithMany()
                .HasForeignKey(r => r.SellerId)
                .WillCascadeOnDelete(false);

            // Add indexes for better performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Configure string properties
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderStatus)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Category)
                .HasMaxLength(100);

            // Configure Notification relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithRequired(n => n.User)
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(true); // Delete notifications when user is deleted

            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Priority)
                .HasMaxLength(20);

            // Configure SavedCard relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.SavedCards)
                .WithRequired(sc => sc.User)
                .HasForeignKey(sc => sc.UserId)
                .WillCascadeOnDelete(true); // Delete saved cards when user is deleted

            modelBuilder.Entity<SavedCard>()
                .Property(sc => sc.CardholderName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<SavedCard>()
                .Property(sc => sc.LastFourDigits)
                .IsRequired()
                .HasMaxLength(4);

            modelBuilder.Entity<SavedCard>()
                .Property(sc => sc.CardType)
                .IsRequired()
                .HasMaxLength(50);

            base.OnModelCreating(modelBuilder);
        }

        public static BookstoreDbContext Create()
        {
            return new BookstoreDbContext();
        }
    }
}