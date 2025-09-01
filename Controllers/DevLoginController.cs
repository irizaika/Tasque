using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// Login controller for development purposes
namespace Tasque.Controllers
{
    public class DevLoginController : Controller
    {
        [HttpGet("/DevLogin")]
        public IActionResult Index()
        {
            // Show a simple login form with tenant + role selection
            return View();
        }

        [HttpPost("/DevLogin")]
        public async Task<IActionResult> Login(string email, string tenant, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("tenant_id", tenant)
            };

            var identity = new ClaimsIdentity(claims, "DevScheme");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("DevScheme", principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("/DevLogout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("DevScheme");
            return Redirect("/DevLogin");
        }
    }
}
