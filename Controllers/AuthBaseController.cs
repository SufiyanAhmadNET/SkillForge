using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SkillForge.Controllers
{
    public class AuthBaseController : Controller
    {
        // string Id — Identity uses GUID string, not int
        protected async Task SigninUser(string Id, string Email, string Role, string PhotoPath)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, Email ?? string.Empty),
                new Claim(ClaimTypes.Role, Role ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, Id ?? string.Empty),
                new Claim("PhotoPath", PhotoPath ?? "/images/DefaultProfilePhoto.jfif")
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            // session backup
            HttpContext.Session.SetString("UserRole", Role ?? string.Empty);
            HttpContext.Session.SetString("UserId", Id ?? string.Empty);
            HttpContext.Session.SetString("UserEmail", Email ?? string.Empty);
            HttpContext.Session.SetString("UserPhotoPath", PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
        }

        // Id is string (Identity GUID)
        protected string? CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(idClaim)) return idClaim;
            return HttpContext.Session.GetString("UserId");
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
            if (!string.IsNullOrEmpty(photo)) return photo;
            return HttpContext.Session.GetString("UserPhotoPath") ?? "/images/DefaultProfilePhoto.jfif";
        }
    }
}