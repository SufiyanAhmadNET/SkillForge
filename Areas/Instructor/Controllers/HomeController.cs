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

        ////Add Course Post
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public IActionResult AddCourse(CourseVM courseVM, IFormFile Thumbnail_File, IFormFile Video_File, string YouTubeUrl, string OutcomesRaw)
        {
            //instructor id from claim
            var InstructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            //split input outcomes into liste by line
            if (!string.IsNullOrWhiteSpace(OutcomesRaw))
            {
                courseVM.outcome = OutcomesRaw
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .ToList();
            }

            //pass to service

            var course = _courseService.AddCourse(courseVM, InstructorId, Thumbnail_File, Video_File, YouTubeUrl);

            if (course.message == CourseMessage.EmptyFields)
            {
                TempData["Alert"] = "Please Enter All Details and in Correct Format ";
                TempData["AlertType"] = "info";
                return RedirectToAction("AddCourse", "Home");
            }
            if (!course.Success)
            {
                ViewBag.ErrorMessage = course.message.ToString();
                return View(courseVM);
            }

            if (!course.Success)
            {
                ViewBag.ErrorMessage = !string.IsNullOrEmpty(course.TechnicalMessage)
                    ? course.TechnicalMessage
                    : course.message.ToString();

                TempData["Alert"] = "Course NOT Added";
                TempData["AlertType"] = "error";
                return RedirectToAction("AddCourse", "Home");
            }

            TempData["Alert"] = "Course Added Successfully";
            TempData["AlertType"] = "success";
            return RedirectToAction("AddCourse", "Home");
        }


        // Get Courses List - Instructor
        [Authorize(Roles ="Instructor")]
        public IActionResult MyCourses()
        {
            var InstructorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var mycourse = _courseService.MyCourses(InstructorId);

            return View(mycourse);
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
