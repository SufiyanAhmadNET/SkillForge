using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
                new Claim (ClaimTypes.Email, Email ?? string.Empty),
                new Claim (ClaimTypes.Role, Role ?? string.Empty),
                new Claim (ClaimTypes.NameIdentifier, Id.ToString()),
                new Claim("PhotoPath", PhotoPath ?? "/images/DefaultProfilePhoto.jfif")
            };

            //Claim Identity
            var identity = new ClaimsIdentity(claims, "Cookies");

            //Wrap Identity in Principle
            var principal = new ClaimsPrincipal(identity);

            //Sign in Request 
            await HttpContext.SignInAsync("Cookies", principal);

            //Session
            HttpContext.Session.SetString("UserRole", Role ?? string.Empty);
            HttpContext.Session.SetInt32("UserId", Id);
            HttpContext.Session.SetString("UserEmail", Email ?? string.Empty);
            HttpContext.Session.SetString("UserPhotoPath", PhotoPath ?? "/images/DefaultProfilePhoto.jfif");

        }

        // Helpers to read current user info
        protected int? CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out var id))
            {
                return id;
            }
            return HttpContext.Session.GetInt32("UserId");
        }

        protected string CurrentUserEmail()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email)) return email;
            return HttpContext.Session.GetString("UserEmail") ?? string.Empty;
        }

        protected string CurrentUserRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(role)) return role;
            return HttpContext.Session.GetString("UserRole") ?? string.Empty;
        }

        protected string CurrentUserPhotoPath()
        {
            var photo = User.FindFirst("PhotoPath")?.Value;
            if (!string.IsNullOrEmpty(photo))
            return photo;

            return HttpContext.Session.GetString("UserPhotoPath") ?? "/images/DefaultProfilePhoto.jfif";
        }
    }
}
