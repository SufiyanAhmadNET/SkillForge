using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SkillForge.Data;
using SkillForge.Controllers;
using System;

namespace SkillForge.Areas.Instructor.Controllers
{
    public class InstructorBaseController : AuthBaseController
    {
        protected readonly SkillForgeDbContext _context;

        public InstructorBaseController(SkillForgeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var id = CurrentUserId(); // string (Identity GUID)

            if (!string.IsNullOrEmpty(id))
            {
                // query instructor profile using string Id
                var profile = _context.instructorProfiles.FirstOrDefault(p => p.InstructorId.ToString() == id);

                ViewBag.Email = CurrentUserEmail();
                ViewBag.FirstName = profile?.FirstName;
                ViewBag.LastName = profile?.LastName;
                ViewBag.PhotoPath = profile?.PhotoPath ?? CurrentUserPhotoPath();
            }

            base.OnActionExecuting(context);
        }
    }
}