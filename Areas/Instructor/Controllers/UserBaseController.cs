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
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // read id from claims 
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(idClaim))
            {
                int id = int.Parse(idClaim);
                var profile = _context.instructorProfiles.FirstOrDefault(p => p.InstructorId == id);

                ViewBag.Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                ViewBag.FirstName = profile?.FirstName;
                ViewBag.LastName = profile?.LastName;
                ViewBag.PhotoPath = profile?.PhotoPath;
            }

            base.OnActionExecuting(context);
        }
    }
}
