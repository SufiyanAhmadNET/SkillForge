using Microsoft.AspNetCore.Mvc;

namespace SkillForge.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        //Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        //Panding Approvals
        public IActionResult Pending_Approvals()
        {
            return View();
        }

        //Admin Courses Dashboard
        public IActionResult Courses()
        {
            return View();
        }

        //Student List for Admin
        public IActionResult Students()
        {
            return View();
        }

        //Instructor List for Admin
        public IActionResult Instructors()
        {
            return View();
        }

        //Instructor Profile/ Details
        public IActionResult Instructor_Profile()
        {
            return View();
        }

        //Reports
        public IActionResult Reports()
        {
            return View();
        }
    }
}
