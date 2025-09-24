using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly BookstoreDbContext _db;

        public AccountController()
        {
            _db = new BookstoreDbContext();
            _authService = new AuthService(_db);
        }

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _authService.Login(model.Username, model.Password);
                if (user != null)
                {
                    _authService.SignIn(user, model.RememberMe);
                    
                    // Redirect based on role
                    switch (user.Role)
                    {
                        case "Admin":
                            return RedirectToAction("Dashboard", "Admin");
                        case "Seller":
                            return RedirectToAction("Dashboard", "Seller");
                        case "Employee":
                            return RedirectToAction("Dashboard", "Employee");
                        default:
                            return RedirectToLocal(returnUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            return View(model);
        }

        // GET: Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Province = model.Province,
                    PostalCode = model.PostalCode,
                    Role = model.Role
                };

                if (_authService.Register(user, model.Password))
                {
                    // Send confirmation email (implement email service later)
                    // For now, auto-confirm for testing
                    _authService.ConfirmEmail(user.Email);
                    
                    // Auto-login after registration
                    _authService.SignIn(user);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Registration failed. Username or email may already be taken.");
                }
            }

            return View(model);
        }

        // POST: Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(WebApplication1.ViewModels.ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    // Generate password reset token (simplified for demo)
                    var resetToken = Guid.NewGuid().ToString();
                    
                    // In a real application, you would:
                    // 1. Store the token in the database with expiration
                    // 2. Send an email with the reset link
                    // 3. Create a proper reset password page
                    
                    ViewBag.Message = "If an account exists with this email, you will receive a password reset link shortly.";
                    
                    // For demo purposes, show a simple message
                    // In production, implement proper email service
                    TempData["SuccessMessage"] = "Password reset instructions have been sent to your email.";
                    return RedirectToAction("Login");
                }
                else
                {
                    // Don't reveal that the email doesn't exist (security best practice)
                    ViewBag.Message = "If an account exists with this email, you will receive a password reset link shortly.";
                }
            }

            return View(model);
        }

        // GET: Account/Profile
        [Authorize]
        public new ActionResult Profile()
        {
            var user = _authService.GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode
            };

            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.Find(model.UserId);
                if (user != null && user.Username == User.Identity.Name)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.Province = model.Province;
                    user.PostalCode = model.PostalCode;

                    _db.SaveChanges();
                    ViewBag.Message = "Profile updated successfully!";
                }
            }

            return View(model);
        }

        // GET: Account/ChangePassword
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _authService.GetCurrentUser();
                if (user != null && _authService.ChangePassword(user.UserId, model.CurrentPassword, model.NewPassword))
                {
                    ViewBag.Message = "Password changed successfully!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    ModelState.AddModelError("", "Current password is incorrect.");
                }
            }

            return View(model);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}