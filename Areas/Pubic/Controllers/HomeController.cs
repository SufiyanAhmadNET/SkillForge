using Microsoft.AspNetCore.Mvc;

namespace SkillForge.Areas.Pubic.Controllers
{
    [Area("Pubic")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Courses()
        {
            return View();
        }

        //Course Details
        public IActionResult CoursesDetails()
        {
            return View();
        }
    }
}
