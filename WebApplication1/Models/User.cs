using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } // Customer, Seller, Admin, Employee

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Province { get; set; }

        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; } = false;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        // Notification preferences
        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; } = true;

        [Display(Name = "SMS Notifications")]
        public bool SmsNotifications { get; set; } = false;

        [Display(Name = "Push Notifications")]
        public bool PushNotifications { get; set; } = true;

        [Display(Name = "Order Updates")]
        public bool OrderUpdates { get; set; } = true;

        [Display(Name = "Book Alerts")]
        public bool BookAlerts { get; set; } = true;

        [Display(Name = "Account Updates")]
        public bool AccountUpdates { get; set; } = true;

        [Display(Name = "Marketing Emails")]
        public bool MarketingEmails { get; set; } = false;

        [Display(Name = "Low Stock Alerts")]
        public bool LowStockAlerts { get; set; } = true;

        [Display(Name = "Price Drop Alerts")]
        public bool PriceDropAlerts { get; set; } = true;

        // Password reset fields
        [StringLength(256)]
        public string PasswordResetToken { get; set; }

        [Display(Name = "Password Reset Token Expiry")]
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Book> Books { get; set; } // For sellers
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } // User notifications
        public virtual ICollection<SavedCard> SavedCards { get; set; } // User saved payment cards

        [NotMapped]
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }

    public enum UserRole
    {
        Customer,
        Seller,
        Admin,
        Employee
    }
}