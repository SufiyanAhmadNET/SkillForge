using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Models;
using SkillForge.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SkillForge.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class HomeController : Controller
    {
        private readonly CourseService _courseService;
        private readonly IConfiguration _config;


        public HomeController(CourseService courseService, IConfiguration config)
        {
            _courseService = courseService;
            _config = config;
        }

        // Instructor Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }


        //Add Course Get
        public IActionResult AddCourse()
        {
            return View();
        }

        //TEMPORARY DATA FOR TESTING
        [HttpPost]
        public IActionResult AddCourse(CourseVM courseVM, string OutcomesRaw)
        {
            // TEMP TEST DATA - remove after testing
            courseVM.category_id = 2;
            courseVM.Difficulty = Course_Difficulty.Beginner;
            courseVM.outcome = new List<string> { "Learn C#", "Learn MVC" };
            courseVM.Title = "Test Course";
            courseVM.Actual_Price = 999;
            courseVM.Duration_Weeks = 8;
            courseVM.Discount_Percent = 10;

            var InstructorId = 1; 

            var course = _courseService.AddCourse(courseVM, InstructorId, null);
            var msg = course.message;
            var success = course.Success;

            return RedirectToAction("AddCourse", "Home");
        }


        //////Add Course Post
        //[Authorize(Roles = "Instructor")]
        //[HttpPost]
        //public IActionResult AddCourse(CourseVM courseVM, IFormFile Thumbnail_File, string OutcomesRaw)
        //{


        //    //instructor id from claim
        //    var InstructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        //    Course_Category category = courseVM.category_id;

        //    //split input comes into liste by line
        //    if (!string.IsNullOrWhiteSpace(OutcomesRaw))
        //    {
        //        courseVM.outcome = OutcomesRaw
        //            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
        //            .Select(o => o.Trim())
        //            .Where(o => !string.IsNullOrWhiteSpace(o))
        //            .ToList();
        //    }

        //    //pass to service

        //    var course = _courseService.AddCourse(courseVM, InstructorId, Thumbnail_File);

        //    if (course.message == CourseMessage.EmptyFields)
        //    {
        //        TempData["EmptyFields"] = "Please Enter All Details and in Correct Format ";
        //        return RedirectToAction("AddCourse", "Home");
        //    }
        //    if (!course.Success)
        //    {
        //        ViewBag.ErrorMessage = course.message.ToString();
        //        return View(courseVM);
        //    }

        //    if (!course.Success)
        //    {          
        //        ViewBag.ErrorMessage = !string.IsNullOrEmpty(course.TechnicalMessage)
        //            ? course.TechnicalMessage
        //            : course.message.ToString();

        //        return View(courseVM);
        //    }
        //    return RedirectToAction("AddCourse", "Home");
        //}


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
