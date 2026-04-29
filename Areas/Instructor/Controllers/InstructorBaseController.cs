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

        // override was missing — that's why photo never loaded
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var id = CurrentUserId(); // string (Identity GUID)

            if (!string.IsNullOrEmpty(id) && _context is not null)
            {
                // query profile using string Id
                var profile = _context.instructorProfiles.FirstOrDefault(p => p.InstructorId.ToString()== id);

                ViewBag.Email = CurrentUserEmail();
                ViewBag.FirstName = profile?.FirstName;
                ViewBag.LastName = profile?.LastName;
                ViewBag.PhotoPath = profile?.PhotoPath ?? CurrentUserPhotoPath();
            }

            base.OnActionExecuting(context);
        }
    }
}