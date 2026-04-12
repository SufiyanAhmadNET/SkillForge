
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SkillForge.Controllers;
using SkillForge.Data;

namespace SkillForge.Areas.Instructor.Controllers
{
    public class InstructorBaseController : AuthBaseController
    {
        [FromServices]
        public required SkillForgeDbContext _context { get; set; }

        protected  void OnActionExecuting(ActionExecutingContext context)
        {
            // Prefer claims (cookie) but fall back to session using helpers on AuthBaseController
            var id = CurrentUserId();
            if (id.HasValue && _context is not null)
            {
                var profile = _context.instructorProfiles.FirstOrDefault(p => p.InstructorId == id.Value);

                ViewBag.Email = CurrentUserEmail();
                ViewBag.FirstName = profile?.FirstName;
                ViewBag.LastName = profile?.LastName;
                ViewBag.PhotoPath = CurrentUserPhotoPath();
            }

            base.OnActionExecuting(context);
        }
    }
}