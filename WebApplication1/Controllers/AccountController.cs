using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // GET: /Account/TestRegister
        [AllowAnonymous]
        public ActionResult TestRegister()
        {
            return View();
        }

        // GET: /Account/DirectTest
        [AllowAnonymous]
        public ActionResult DirectTest()
        {
            return View();
        }

        // POST: Account/TestDebug
        [HttpPost]
        [AllowAnonymous]
        public ActionResult TestDebug(RegisterViewModel model)
        {
            var debugInfo = new StringBuilder();
            debugInfo.AppendLine("=== TEST DEBUG ACTION CALLED ===");
            debugInfo.AppendLine($"ModelState.IsValid: {ModelState.IsValid}");
            debugInfo.AppendLine($"FirstName: {model.FirstName}");
            debugInfo.AppendLine($"LastName: {model.LastName}");
            debugInfo.AppendLine($"Email: {model.Email}");
            debugInfo.AppendLine($"Username: {model.Username}");
            debugInfo.AppendLine($"Role: {model.Role}");
            debugInfo.AppendLine($"AcceptTerms: {model.AcceptTerms}");
            debugInfo.AppendLine($"Password provided: {!string.IsNullOrEmpty(model.Password)}");
            debugInfo.AppendLine($"ConfirmPassword provided: {!string.IsNullOrEmpty(model.ConfirmPassword)}");
            debugInfo.AppendLine($"Passwords match: {model.Password == model.ConfirmPassword}");
            
            if (!ModelState.IsValid)
            {
                var errorMessages = new List<string>();
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errorMessages.Add($"{key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                debugInfo.AppendLine("ModelState is INVALID. Errors:");
                debugInfo.AppendLine(string.Join("\n", errorMessages));
            }
            
            debugInfo.AppendLine("=== END DEBUG INFO ===");
            
            // Return the debug info directly as content
            return Content(debugInfo.ToString(), "text/plain");
        }

        // GET: Account/TestDebug
        [AllowAnonymous]
        public ActionResult TestDebug()
        {
            return Content("TestDebug GET action - ready for POST requests");
        }

        // GET: Account/TestSimple
        [AllowAnonymous]
        public ActionResult TestSimple()
        {
            return Content("TestSimple action is working!");
        }

        // POST: Account/TestSimple
        [HttpPost]
        [AllowAnonymous]
        public ActionResult TestSimple(string testData)
        {
            return Content($"TestSimple POST received: {testData}");
        }

        // Simple test action to verify POST is working
        [HttpPost]
        [AllowAnonymous]
        public ActionResult TestPost()
        {
            try
            {
                return Json(new { success = true, message = "POST test successful", timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // GET: Account/SimpleTest
        [AllowAnonymous]
        public ActionResult SimpleTest()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            // Add debugging to see what data is received
            var debugInfo = new StringBuilder();
            debugInfo.AppendLine($"=== REGISTER ACTION CALLED === ModelState.IsValid: {ModelState.IsValid}");
            debugInfo.AppendLine($"Timestamp: {DateTime.Now}");
            
            // Log all model properties to check for null values
            debugInfo.AppendLine("=== MODEL PROPERTIES ===");
            debugInfo.AppendLine($"FirstName: {model.FirstName ?? "NULL"}");
            debugInfo.AppendLine($"LastName: {model.LastName ?? "NULL"}");
            debugInfo.AppendLine($"Email: {model.Email ?? "NULL"}");
            debugInfo.AppendLine($"Username: {model.Username ?? "NULL"}");
            debugInfo.AppendLine($"Password: {(string.IsNullOrEmpty(model.Password) ? "NULL_OR_EMPTY" : "PROVIDED")}");
            debugInfo.AppendLine($"ConfirmPassword: {(string.IsNullOrEmpty(model.ConfirmPassword) ? "NULL_OR_EMPTY" : "PROVIDED")}");
            debugInfo.AppendLine($"Role: {model.Role ?? "NULL"}");
            debugInfo.AppendLine($"AcceptTerms: {model.AcceptTerms}");
            debugInfo.AppendLine($"PhoneNumber: {model.PhoneNumber ?? "NULL"}");
            debugInfo.AppendLine($"Address: {model.Address ?? "NULL"}");
            debugInfo.AppendLine($"City: {model.City ?? "NULL"}");
            debugInfo.AppendLine($"Province: {model.Province ?? "NULL"}");
            debugInfo.AppendLine($"PostalCode: {model.PostalCode ?? "NULL"}");
            
            if (!ModelState.IsValid)
            {
                var errorMessages = new List<string>();
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errorMessages.Add($"{key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                debugInfo.AppendLine("ModelState is INVALID. Errors:");
                debugInfo.AppendLine(string.Join("\n", errorMessages));
            }
            else
            {
                debugInfo.AppendLine("ModelState is VALID");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    debugInfo.AppendLine("Creating user object...");
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

                    debugInfo.AppendLine("User object created successfully");
                    debugInfo.AppendLine($"User details - Email: {user.Email}, Username: {user.Username}, Role: {user.Role}");
                    
                    if (_authService != null)
                    {
                        debugInfo.AppendLine("AuthService is available, attempting registration...");
                        
                        if (_authService.Register(user, model.Password))
                        {
                            debugInfo.AppendLine("Registration SUCCESSFUL");
                            TempData["DebugInfo"] = debugInfo.ToString();
                            
                            // Send confirmation email (implement email service later)
                            // For now, auto-confirm for testing
                            try
                            {
                                debugInfo.AppendLine("Attempting to confirm email...");
                                _authService.ConfirmEmail(user.Email);
                                debugInfo.AppendLine("Email confirmation SUCCESSFUL");
                            }
                            catch (Exception emailEx)
                            {
                                debugInfo.AppendLine($"Email confirmation failed: {emailEx.Message}");
                                // Don't fail registration if email confirmation fails
                            }
                            
                            // Auto-login after registration
                            try
                            {
                                debugInfo.AppendLine("Attempting to sign in user...");
                                _authService.SignIn(user);
                                debugInfo.AppendLine("Sign in SUCCESSFUL");
                            }
                            catch (Exception signInEx)
                            {
                                debugInfo.AppendLine($"Sign in failed: {signInEx.Message}");
                                // Don't fail if sign in fails, just redirect to login
                                return RedirectToAction("Login", "Account");
                            }
                            
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            debugInfo.AppendLine("Registration FAILED - username or email may already exist");
                            ModelState.AddModelError("", "Registration failed. Username or email may already be taken.");
                        }
                    }
                    else
                    {
                        debugInfo.AppendLine("ERROR: _authService is null!");
                        ModelState.AddModelError("", "Authentication service is not available.");
                    }
                }
                catch (Exception ex)
                {
                    debugInfo.AppendLine($"EXCEPTION during registration: {ex.Message}");
                    debugInfo.AppendLine($"Exception type: {ex.GetType().FullName}");
                    debugInfo.AppendLine($"Stack trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        debugInfo.AppendLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    
                    ModelState.AddModelError("", $"An error occurred during registration: {ex.Message}");
                }
            }
            else
            {
                debugInfo.AppendLine("Returning view with model errors");
            }

            // Always set the debug info before returning
            TempData["DebugInfo"] = debugInfo.ToString();
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

        // POST: Account/TestDebug
        [HttpPost]
        [AllowAnonymous]
        public ActionResult TestDebug(FormCollection form)
        {
            var debugInfo = new StringBuilder();
            debugInfo.AppendLine("=== TestDebug Action Called ===");
            debugInfo.AppendLine($"Timestamp: {DateTime.Now}");
            debugInfo.AppendLine($"ModelState.IsValid: {ModelState.IsValid}");
            debugInfo.AppendLine($"Form keys count: {form.Count}");
            
            foreach (string key in form.Keys)
            {
                debugInfo.AppendLine($"Form[{key}]: {form[key]}");
            }
            
            debugInfo.AppendLine("=== ModelState Errors ===");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Count > 0)
                {
                    debugInfo.AppendLine($"{key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            return Content(debugInfo.ToString(), "text/plain");
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