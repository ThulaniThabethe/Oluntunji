using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebApplication1.Models;
using WebApplication1.Helpers;

namespace WebApplication1.Services
{
    public class AuthService
    {
        private readonly BookstoreDbContext _db;

        public AuthService(BookstoreDbContext db)
        {
            _db = db;
        }

        public User Login(string username, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
            if (user != null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                user.LastLoginDate = DateTime.Now;
                _db.SaveChanges();
                return user;
            }
            return null;
        }

        public bool Register(User user, string password)
        {
            try
            {
                // Check if username or email already exists
                if (_db.Users.Any(u => u.Username == user.Username || u.Email == user.Email))
                {
                    return false;
                }

                user.PasswordHash = PasswordHelper.HashPassword(password);
                user.CreatedDate = DateTime.Now;
                user.EmailConfirmed = false;
                user.IsActive = true;

                // Set default role if not specified
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = UserRole.Customer.ToString();
                }

                _db.Users.Add(user);
                _db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _db.Users.Find(userId);
            if (user != null && PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash))
            {
                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                _db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool ResetPassword(string email, string newPassword)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                _db.SaveChanges();
                return true;
            }
            return false;
        }

        public User GetCurrentUser()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var username = HttpContext.Current.User.Identity.Name;
                return _db.Users.FirstOrDefault(u => u.Username == username);
            }
            return null;
        }

        public bool IsUserInRole(string role)
        {
            var user = GetCurrentUser();
            return user != null && user.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        public bool HasPermission(string requiredRole)
        {
            var user = GetCurrentUser();
            if (user == null) return false;

            // Admin has all permissions
            if (user.Role == UserRole.Admin.ToString()) return true;

            // Check if user has the required role
            return user.Role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase);
        }

        public void SignIn(User user, bool rememberMe = false)
        {
            FormsAuthentication.SetAuthCookie(user.Username, rememberMe);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public bool ConfirmEmail(string email)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.EmailConfirmed = true;
                _db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool IsEmailConfirmed(string email)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            return user?.EmailConfirmed ?? false;
        }
    }
}