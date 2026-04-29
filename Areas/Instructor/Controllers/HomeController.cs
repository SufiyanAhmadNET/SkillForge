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
        public IActionResult AddCourse(CourseVM courseVM, IFormFile thumbnail_url, IFormFile Video_File, string YouTubeUrl, string videoType, string OutcomesRaw, string action)
        {
            //instructor id from claim
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var InstructorId))
            {
                TempData["Alert"] = "Session expired. Please login again.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorLogin", "Auth");
            }

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
            var course = _courseService.AddCourse(courseVM, InstructorId, thumbnail_url, Video_File, YouTubeUrl, videoType, action);

            if (course.message == CourseMessage.EmptyFields)
            {
                TempData["Alert"] = !string.IsNullOrWhiteSpace(course.TechnicalMessage)
                    ? course.TechnicalMessage
                    : "Please enter all required course details in the correct format.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("AddCourse", "Home");
            }

            if (!course.Success)
            {
                TempData["Alert"] = !string.IsNullOrWhiteSpace(course.TechnicalMessage)
                    ? course.TechnicalMessage
                    : "Course could not be added. Please check your input and try again.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("AddCourse", "Home");
            }

            TempData["Alert"] = action?.ToLower() == "submit"
                ? "Course submitted successfully for review."
                : "Course saved to draft successfully.";
            TempData["AlertType"] = "success";
            return RedirectToAction("MyCourses", "Home");
        }


        // Get Courses List - Instructor
        [Authorize(Roles ="Instructor")]
        public IActionResult MyCourses()
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var InstructorId))
            {
                TempData["Alert"] = "Session expired. Please login again.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorLogin", "Auth");
            }
            var mycourse = _courseService.MyCourses(InstructorId);

            return View(mycourse);
        }

        // Course details - instructor view with students
        [Authorize(Roles = "Instructor")]
        public IActionResult CourseDetails(int id)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            var courseDetails = _courseService.GetInstructorCourseDetails(id, instructorId);
            if (courseDetails == null)
                return NotFound();

            return View(courseDetails);
        }

        //edit course  
        [Authorize(Roles = "Instructor")]
        public IActionResult EditCourse(int CourseId)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            var courseDetails = _courseService.GetInstructorCourseDetails(CourseId, instructorId);
            if (courseDetails == null)
                return NotFound();

            return View(courseDetails);
        }

        // Delete course 
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public IActionResult DeleteCourse(int id)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            // Delete course
            bool deleted = _courseService.DeleteCourse(id, instructorId);

            if (deleted)
            {
                TempData["Alert"] = "Course deleted successfully.";
                TempData["AlertType"] = "success";
            }
            else
            {
                TempData["Alert"] = "Failed to delete course. Check permissions.";
                TempData["AlertType"] = "danger";
            }

            return RedirectToAction("MyCourses");
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
