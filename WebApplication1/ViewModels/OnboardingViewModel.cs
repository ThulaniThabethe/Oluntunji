using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebApplication1.ViewModels
{
    public class OnboardingViewModel
    {
        // Personal Information
        [Required]
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Display(Name = "Profile Image")]
        public HttpPostedFileBase ProfileImage { get; set; }

        // Contact Details
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Alternate Phone")]
        [StringLength(20)]
        public string AlternatePhone { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(500)]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(500)]
        public string AddressLine2 { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string City { get; set; }

        [Display(Name = "Province")]
        [StringLength(100)]
        public string Province { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(10)]
        public string PostalCode { get; set; }

        // Role Setup
        public string CurrentRole { get; set; }

        // Seller Information
        [Display(Name = "Business Name")]
        [StringLength(200)]
        public string SellerBusinessName { get; set; }

        [Display(Name = "Business Description")]
        [StringLength(1000)]
        public string SellerDescription { get; set; }

        [Display(Name = "Website")]
        [Url]
        [StringLength(200)]
        public string SellerWebsite { get; set; }

        // Employee Information
        [Display(Name = "Department")]
        [StringLength(100)]
        public string EmployeeDepartment { get; set; }

        [Display(Name = "Position")]
        [StringLength(100)]
        public string EmployeePosition { get; set; }

        [Display(Name = "Employee ID")]
        [StringLength(50)]
        public string EmployeeId { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? EmployeeStartDate { get; set; }

        // Customer Information
        [Display(Name = "Preferred Genres")]
        public List<string> CustomerPreferredGenres { get; set; }

        [Display(Name = "Reading Preferences")]
        [StringLength(500)]
        public string CustomerReadingPreferences { get; set; }

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime? CustomerBirthDate { get; set; }

        // Additional Information
        [Display(Name = "Bio")]
        [StringLength(1000)]
        public string Bio { get; set; }

        [Display(Name = "Notification Preferences")]
        public string NotificationPreferences { get; set; }

        [Display(Name = "Privacy Settings")]
        public string PrivacySettings { get; set; }

        // Constructor
        public OnboardingViewModel()
        {
            CustomerPreferredGenres = new List<string>();
        }
    }
}