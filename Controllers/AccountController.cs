using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Tasque.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = returnUrl
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/"
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }

}
