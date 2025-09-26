using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebApplication1.ViewModels
{
    public class SavedCardViewModel
    {
        public int CardId { get; set; }

        [Display(Name = "Cardholder Name")]
        public string CardholderName { get; set; }

        [Display(Name = "Card Number")]
        public string MaskedCardNumber { get; set; }

        [Display(Name = "Card Type")]
        public string CardType { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }
    }

    public class SavedCardsListViewModel
    {
        public List<SavedCardViewModel> SavedCards { get; set; }
        public int TotalCards { get; set; }
        public bool HasDefaultCard { get; set; }
    }

    public class AddCardViewModel
    {
        [Required(ErrorMessage = "Cardholder name is required")]
        [Display(Name = "Cardholder Name")]
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        public string CardholderName { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$", ErrorMessage = "Please enter a valid 16-digit card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry month is required")]
        [Display(Name = "Expiry Month")]
        [Range(1, 12, ErrorMessage = "Please select a valid month")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required")]
        [Display(Name = "Expiry Year")]
        [Range(2024, 2050, ErrorMessage = "Please select a valid year")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Please enter a valid 3 or 4-digit CVV")]
        public string CVV { get; set; }

        [Display(Name = "Set as Default")]
        public bool SetAsDefault { get; set; }
    }

    public class EditCardViewModel
    {
        public int CardId { get; set; }

        [Required(ErrorMessage = "Cardholder name is required")]
        [Display(Name = "Cardholder Name")]
        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        public string CardholderName { get; set; }

        [Required(ErrorMessage = "Expiry month is required")]
        [Display(Name = "Expiry Month")]
        [Range(1, 12, ErrorMessage = "Please select a valid month")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required")]
        [Display(Name = "Expiry Year")]
        [Range(2024, 2050, ErrorMessage = "Please select a valid year")]
        public int ExpiryYear { get; set; }

        [Display(Name = "Card Type")]
        public string CardType { get; set; }

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; }

        [Display(Name = "Set as Default")]
        public bool SetAsDefault { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public List<CardTypeViewModel> AvailableCardTypes { get; set; }
    }

    public class CardTypeViewModel
    {
        public string CardType { get; set; }
        public string CardTypeDisplayName { get; set; }
        public bool IsSupported { get; set; }
    }

    public class NotificationViewModel
    {
        public int NotificationId { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Type")]
        public string NotificationType { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; }

        [Display(Name = "Is Read")]
        public bool IsRead { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Related Link")]
        public string RelatedLink { get; set; }

        [Display(Name = "Related ID")]
        public int? RelatedId { get; set; }
    }

    public class NotificationsListViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadCount { get; set; }
        public int HighPriorityCount { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; }
    }

    public class NotificationSettingsViewModel
    {
        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "SMS Notifications")]
        public bool SmsNotifications { get; set; }

        [Display(Name = "Push Notifications")]
        public bool PushNotifications { get; set; }

        [Display(Name = "Order Updates")]
        public bool OrderUpdates { get; set; }

        [Display(Name = "Book Alerts")]
        public bool BookAlerts { get; set; }

        [Display(Name = "Account Updates")]
        public bool AccountUpdates { get; set; }

        [Display(Name = "Marketing Emails")]
        public bool MarketingEmails { get; set; }

        [Display(Name = "Low Stock Alerts")]
        public bool LowStockAlerts { get; set; }

        [Display(Name = "Price Drop Alerts")]
        public bool PriceDropAlerts { get; set; }
    }

    public class ContactViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(100, ErrorMessage = "Subject cannot be longer than 100 characters")]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot be longer than 1000 characters")]
        [MinLength(10, ErrorMessage = "Message must be at least 10 characters long")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Subscribe to Newsletter")]
        public bool Newsletter { get; set; }
    }

    public class AdminSavedCardViewModel
    {
        public int CardId { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CardType { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        [Display(Name = "Masked Card Number")]
        public string MaskedCardNumber
        {
            get
            {
                if (string.IsNullOrEmpty(CardNumber))
                    return "****";
                
                // Show only last 4 digits
                var cleanNumber = CardNumber.Replace(" ", "").Replace("-", "");
                if (cleanNumber.Length >= 4)
                    return $"**** **** **** {cleanNumber.Substring(cleanNumber.Length - 4)}";
                
                return "****";
            }
        }

        [Display(Name = "Is Expired")]
        public bool IsExpired
        {
            get
            {
                var currentDate = DateTime.Now;
                var cardYear = 2000 + ExpiryYear; // Assuming 2-digit year format
                return cardYear < currentDate.Year || (cardYear == currentDate.Year && ExpiryMonth < currentDate.Month);
            }
        }
    }

    public class AdminSavedCardsListViewModel
    {
        public List<AdminSavedCardViewModel> SavedCards { get; set; }
        public int TotalCards { get; set; }
    }
}