using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        protected AuthService AuthService;
        protected BookstoreDbContext Db;

        public BaseController()
        {
            Db = new BookstoreDbContext();
            AuthService = new AuthService(Db);
        }

        protected User CurrentUser
        {
            get
            {
                return AuthService.GetCurrentUser();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Custom authorization attributes
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }

    public class AdminOnlyAttribute : AuthorizeRoleAttribute
    {
        public AdminOnlyAttribute() : base("Admin") { }
    }

    public class SellerOnlyAttribute : AuthorizeRoleAttribute
    {
        public SellerOnlyAttribute() : base("Seller") { }
    }

    public class EmployeeOnlyAttribute : AuthorizeRoleAttribute
    {
        public EmployeeOnlyAttribute() : base("Employee") { }
    }

    public class CustomerOnlyAttribute : AuthorizeRoleAttribute
    {
        public CustomerOnlyAttribute() : base("Customer") { }
    }

    public class AdminOrSellerAttribute : AuthorizeRoleAttribute
    {
        public AdminOrSellerAttribute() : base("Admin", "Seller") { }
    }

    public class AdminOrEmployeeAttribute : AuthorizeRoleAttribute
    {
        public AdminOrEmployeeAttribute() : base("Admin", "Employee") { }
    }
}