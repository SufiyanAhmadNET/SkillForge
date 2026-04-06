using Microsoft.AspNetCore.Mvc;

namespace SkillForge.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class HomeController : Controller
    {
         // Instructor Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        //Add Course
        public IActionResult AddCourse()   
        {
            return View();
        }

        //Instructor Courses
        public IActionResult MyCourses()
        {
            return View();
        }

        //Course detail -view
        public IActionResult CourseDetails()
        {
            return View();
        }


        public IActionResult EditCourses()
        {
            return View();
        }

        //My Earning
        public IActionResult Earning()
        {
            return View();
        }

        //Instructor Profile
        public IActionResult Profile()
        {
            return View();
        }


    }
}
