using System;
using System.Linq;
using System.Web.Security;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CustomRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }

        public override string[] GetRolesForUser(string username)
        {
            using (var db = new BookstoreDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
                return user != null ? new string[] { user.Role } : new string[0];
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var db = new BookstoreDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
                return user != null && user.Role.Equals(roleName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var db = new BookstoreDbContext())
            {
                var users = db.Users.Where(u => u.Role == roleName && u.IsActive).Select(u => u.Username).ToArray();
                return users;
            }
        }

        public override bool RoleExists(string roleName)
        {
            var roles = new[] { "Customer", "Seller", "Admin", "Employee" };
            return roles.Contains(roleName);
        }

        #region Not Implemented Methods
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            return new[] { "Customer", "Seller", "Admin", "Employee" };
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}