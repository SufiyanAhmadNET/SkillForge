using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SkillForge.Controllers
{
    public class AuthBaseController : Controller
    {
        // shared SigninUser - can call in any area
        protected async Task SigninUser(int Id, string Email, string Role, string PhotoPath)
        {
            //Claims to Manage Account

            var claims = new List<Claim>
            {
                new Claim (ClaimTypes.Email,Email),
                new Claim (ClaimTypes.Role,Role),
                new Claim (ClaimTypes.NameIdentifier,Id.ToString()),
                 new Claim("PhotoPath", PhotoPath ?? "/images/DefaultProfilePhoto.jfif")
            };

            //Claim Identity
            var identity = new ClaimsIdentity(claims, "Cookies");

            //Wrap Identity in Principle
            var principal = new ClaimsPrincipal(identity);

            //Sign in Request 
            await HttpContext.SignInAsync("Cookies", principal);

            //Session
            HttpContext.Session.SetString("UserRole", Role);
            HttpContext.Session.SetInt32("UserId", Id);
            HttpContext.Session.SetString("UserEmail", Email);
            HttpContext.Session.SetString("UserPhotoPath", PhotoPath ?? "/images/DefaultProfilePhoto.jfif");

        }
    }
}
