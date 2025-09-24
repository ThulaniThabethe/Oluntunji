using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string City { get; set; }

        [Display(Name = "Province")]
        [StringLength(100)]
        public string Province { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(10)]
        public string PostalCode { get; set; }
    }

    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(100)]
        public string Username { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string City { get; set; }

        [Display(Name = "Province")]
        [StringLength(100)]
        public string Province { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(10)]
        public string PostalCode { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}