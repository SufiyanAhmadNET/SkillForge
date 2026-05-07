using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Services;
using System.Security.Claims;

namespace SkillForge.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class HomeController : InstructorBaseController
    {
        private readonly CourseService _courseService;

        public HomeController(SkillForgeDbContext context, CourseService courseService) : base(context) => _courseService = courseService;

        [Authorize(Roles = "Instructor")]
        public IActionResult Dashboard()
        {
            // get instructor id from claims
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            var vm = _courseService.GetInstructorDashboard(instructorId);
            return View(vm);
        }

        // Add course page
        [Authorize(Roles = "Instructor")]
        public IActionResult AddCourse()
        {
            return View();
        }

        // Add Course - POST
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

        // get course for editing
        [Authorize(Roles = "Instructor")]
        public IActionResult EditCourse(int id)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            var course = _courseService.GetCourseForEdit(id, instructorId);
            if (course == null)
                return NotFound();

            return View(course);
        }

        // update course data
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public IActionResult EditCourse(CourseVM courseVM, IFormFile thumbnail_url, IFormFile Video_File, string YouTubeUrl, string videoType, string OutcomesRaw, string action)
        {
            var instructorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(instructorIdClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            // parse outcomes from raw text
            if (!string.IsNullOrWhiteSpace(OutcomesRaw))
            {
                courseVM.outcome = OutcomesRaw
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .ToList();
            }

            var result = _courseService.UpdateCourse(courseVM, instructorId, thumbnail_url, Video_File, YouTubeUrl, videoType, action);

            if (result.Success)
            {
                TempData["Alert"] = "Course updated successfully.";
                TempData["AlertType"] = "success";
                return RedirectToAction("MyCourses");
            }

            TempData["Alert"] = result.TechnicalMessage ?? "Failed to update course.";
            TempData["AlertType"] = "danger";
            return View(courseVM);
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

        [Authorize(Roles = "Instructor")]
        public IActionResult Profile()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            var instructor = _context.instructors
                .Include(i => i.Profile)
                .FirstOrDefault(i => i.Id == instructorId);

            if (instructor == null) return NotFound();

            var profile = instructor.Profile ?? new InstructorProfile();
            
            // fetch real stats for profile sidebar
            var coursesCount = _context.Courses.Count(c => c.instructor_id == instructorId);
            var studentCount = _context.Enrollments
                .Count(e => _context.Courses.Any(c => c.Id == e.CourseId && c.instructor_id == instructorId) && e.Status == EnrollmentStatus.Active);

            var vm = new InstructorDashboardVM
            {
                Email = instructor.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Mobile = profile.Mobile,
                Location = profile.Location,
                Bio = profile.Bio,
                Profession = profile.Profession,
                Headline = profile.Headline,
                WebsiteUrl = profile.WebsiteUrl,
                GithubUrl = profile.GithubUrl,
                LinkedinUrl = profile.LinkedinUrl,
                TwitterUrl = profile.TwitterUrl,
                Skills = profile.Skills,
                PhotoPath = profile.PhotoPath ?? "/images/DefaultProfilePhoto.jfif",
                TotalCourses = coursesCount,
                TotalStudents = studentCount
            };

            return View(vm);
        }

        // instructor profile - POST
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public IActionResult Profile(InstructorProfile profile, IFormFile PhotoFile)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var instructorId))
                return RedirectToAction("InstructorLogin", "Auth");

            profile.InstructorId = instructorId;

            // photo upload logic
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(PhotoFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", fileName);
                
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    PhotoFile.CopyTo(stream);
                }
                profile.PhotoPath = "/images/profiles/" + fileName;
            }
            else
            {
                var existing = _context.instructorProfiles.FirstOrDefault(p => p.InstructorId == instructorId);
                profile.PhotoPath = existing?.PhotoPath ?? "/images/DefaultProfilePhoto.jfif";
            }

            var existingProfile = _context.instructorProfiles.FirstOrDefault(p => p.InstructorId == instructorId);
            if (existingProfile == null)
            {
                _context.instructorProfiles.Add(profile);
            }
            else
            {
                existingProfile.FirstName = profile.FirstName;
                existingProfile.LastName = profile.LastName;
                existingProfile.Mobile = profile.Mobile;
                existingProfile.Location = profile.Location;
                existingProfile.Bio = profile.Bio;
                existingProfile.Profession = profile.Profession;
                existingProfile.Headline = profile.Headline;
                existingProfile.WebsiteUrl = profile.WebsiteUrl;
                existingProfile.GithubUrl = profile.GithubUrl;
                existingProfile.LinkedinUrl = profile.LinkedinUrl;
                existingProfile.TwitterUrl = profile.TwitterUrl;
                existingProfile.Skills = profile.Skills;
                existingProfile.PhotoPath = profile.PhotoPath;
                _context.instructorProfiles.Update(existingProfile);
            }

            _context.SaveChanges();
            TempData["Alert"] = "Profile updated successfully.";
            TempData["AlertType"] = "success";
            
            return RedirectToAction("Profile");
        }

        // Syllabus Management - Add Module
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult AddModule(int courseId, string moduleName)
        {
            var success = _courseService.AddModule(courseId, moduleName);
            if (success)
            {
                TempData["Alert"] = "Module added successfully.";
                TempData["AlertType"] = "success";
            }
            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        // Syllabus Management - Add Lesson
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult AddLesson(int moduleId, int courseId, string title, string videoUrl)
        {
            var success = _courseService.AddLesson(moduleId, title, videoUrl);
            if (success)
            {
                TempData["Alert"] = "Lesson added successfully.";
                TempData["AlertType"] = "success";
            }
            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        // Syllabus Management - Delete Module
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult DeleteModule(int moduleId, int courseId)
        {
            var success = _courseService.DeleteModule(moduleId);
            if (success)
            {
                TempData["Alert"] = "Module deleted.";
                TempData["AlertType"] = "success";
            }
            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        // Syllabus Management - Delete Lesson
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public IActionResult DeleteLesson(int lessonId, int courseId)
        {
            var success = _courseService.DeleteLesson(lessonId);
            if (success)
            {
                TempData["Alert"] = "Lesson deleted.";
                TempData["AlertType"] = "success";
            }
            return RedirectToAction("CourseDetails", new { id = courseId });
        }


    }
}
