using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SkillForge.Controllers;
using SkillForge.Data;

namespace SkillForge.Areas.User.Controllers
{
    public class UserBaseController : AuthBaseController
    {

        [FromServices]
        public required SkillForgeDbContext _context { get; set; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // claims 
            var id = CurrentUserId();
            if (id.HasValue)
            {
                var profile = _context.StudentProfiles.FirstOrDefault(p => p.StudentId == id.Value);

                ViewBag.Email = CurrentUserEmail(); //helper method
                ViewBag.FirstName = profile?.FirstName;
                ViewBag.LastName = profile?.LastName;
                ViewBag.PhotoPath = CurrentUserPhotoPath(); //helper method
            }

            base.OnActionExecuting(context);
        }
    }
}
