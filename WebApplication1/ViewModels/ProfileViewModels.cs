using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string City { get; set; }

        [StringLength(50, ErrorMessage = "Province cannot exceed 50 characters")]
        [Display(Name = "Province")]
        public string Province { get; set; }

        [StringLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
    }

    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Province")]
        public string Province { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "User Role")]
        public UserRole Role { get; set; }

        [Display(Name = "Date Registered")]
        public DateTime DateRegistered { get; set; }

        [Display(Name = "Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool IsEmailConfirmed { get; set; }
    }

    public class SellerDashboardViewModel
    {
        [Display(Name = "Total Books Listed")]
        public int TotalBooks { get; set; }

        [Display(Name = "Total Orders Received")]
        public int TotalOrders { get; set; }

        [Display(Name = "Total Revenue Generated")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Books Sold")]
        public int BooksSold { get; set; }

        [Display(Name = "Low Stock Books")]
        public List<Book> LowStockBooks { get; set; }

        [Display(Name = "Recent Orders")]
        public List<Order> RecentOrders { get; set; }
    }

    public class AdminDashboardViewModel
    {
        [Display(Name = "Total Users")]
        public int TotalUsers { get; set; }

        [Display(Name = "Total Books")]
        public int TotalBooks { get; set; }

        [Display(Name = "Total Orders")]
        public int TotalOrders { get; set; }

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Pending Orders")]
        public int PendingOrders { get; set; }

        [Display(Name = "Users Registered This Month")]
        public int RegisteredUsersThisMonth { get; set; }

        [Display(Name = "Recent Users")]
        public List<User> RecentUsers { get; set; }

        [Display(Name = "Recent Orders")]
        public List<Order> RecentOrders { get; set; }

        [Display(Name = "Top Selling Books")]
        public List<Book> TopSellingBooks { get; set; }
    }

    public class EmployeeDashboardViewModel
    {
        [Display(Name = "Pending Orders")]
        public int PendingOrders { get; set; }

        [Display(Name = "Shipped Orders")]
        public int ShippedOrders { get; set; }

        [Display(Name = "Revenue This Month")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenueThisMonth { get; set; }

        [Display(Name = "Recent Orders")]
        public List<Order> RecentOrders { get; set; }

        [Display(Name = "Low Stock Books")]
        public List<Book> LowStockBooks { get; set; }

        [Display(Name = "Recent Reviews")]
        public List<Review> RecentReviews { get; set; }
    }

    public class UserManagementViewModel
    {
        [Display(Name = "User ID")]
        public int UserId { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "User Role")]
        public UserRole Role { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime DateRegistered { get; set; }

        [Display(Name = "Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Account Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool IsEmailConfirmed { get; set; }
    }

    public class UserRoleUpdateViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public UserRole NewRole { get; set; }
    }

    public class UserStatusUpdateViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }

    public class UserOrderHistoryViewModel
    {
        [Display(Name = "Order ID")]
        public int OrderId { get; set; }

        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Number of Items")]
        public int ItemCount { get; set; }
    }

    public class CustomerReviewHistoryViewModel
    {
        [Display(Name = "Book Title")]
        public string BookTitle { get; set; }

        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [Display(Name = "Review Text")]
        public string ReviewText { get; set; }

        [Display(Name = "Review Date")]
        public DateTime ReviewDate { get; set; }

        [Display(Name = "Helpful Votes")]
        public int HelpfulVotes { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "User Role")]
        public string Role { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Street Address")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string City { get; set; }

        [StringLength(50, ErrorMessage = "Province cannot exceed 50 characters")]
        [Display(Name = "Province")]
        public string Province { get; set; }

        [StringLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        public List<SelectListItem> AvailableRoles { get; set; }
    }
}