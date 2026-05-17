using Microsoft.AspNetCore.Mvc;
using SkillForge.Interfaces;
using SkillForge.Services.Instructors.Models;
using SkillForge.Areas.Admin.Models;
using SkillForge.Services.Courses.Models;

namespace SkillForge.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IAdminService _adminService;

        public HomeController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            var data = _adminService.GetDashboardData();
            ViewBag.PendingCount = data.RecentApplications.Count(a => a.Status == MentorApplicationStatus.Pending);
            return View(data);
        }

        // Pending Approvals
        public IActionResult Pending_Approvals()
        {
            var applications = _adminService.GetAllMentorApplications();
            ViewBag.PendingCount = applications.Count(a => a.Status == MentorApplicationStatus.Pending);
            return View(applications);
        }

        // Approve Application
        [HttpPost]
        public IActionResult ApproveApplication(int id)
        {
            var result = _adminService.UpdateApplicationStatus(id, MentorApplicationStatus.Approved);
            if (result)
            {
                TempData["Alert"] = "success:Instructor application approved successfully.";
            }
            else
            {
                TempData["Alert"] = "error:Failed to approve application.";
            }
            return RedirectToAction(nameof(Pending_Approvals));
        }

        // Reject Application
        [HttpPost]
        public IActionResult RejectApplication(int id, string? comment)
        {
            var result = _adminService.UpdateApplicationStatus(id, MentorApplicationStatus.Rejected, comment);
            if (result)
            {
                TempData["Alert"] = "success:Instructor application rejected.";
            }
            else
            {
                TempData["Alert"] = "error:Failed to reject application.";
            }
            return RedirectToAction(nameof(Pending_Approvals));
        }

        // Admin Courses Dashboard
        public IActionResult Courses()
        {
            var courses = _adminService.GetAllCoursesForReview();
            ViewBag.PendingCount = _adminService.GetAllMentorApplications().Count(a => a.Status == MentorApplicationStatus.Pending);
            ViewBag.PendingCourseCount = courses.Count(c => c.Status == CourseStatus.PendingReview);
            return View(courses);
        }

        // Approve Course
        [HttpPost]
        public IActionResult ApproveCourse(int id)
        {
            var result = _adminService.UpdateCourseStatus(id, CourseStatus.Approved);
            if (result)
            {
                TempData["Alert"] = "success:Course approved and is now live.";
            }
            else
            {
                TempData["Alert"] = "error:Failed to approve course.";
            }
            return RedirectToAction(nameof(Courses));
        }

        // Reject Course
        [HttpPost]
        public IActionResult RejectCourse(int id, string? reason)
        {
            var result = _adminService.UpdateCourseStatus(id, CourseStatus.Rejected, reason);
            if (result)
            {
                TempData["Alert"] = "success:Course rejected with feedback.";
            }
            else
            {
                TempData["Alert"] = "error:Failed to reject course.";
            }
            return RedirectToAction(nameof(Courses));
        }

        // Student List
        public IActionResult Students()
        {
            var students = _adminService.GetAllStudents();
            ViewBag.PendingCount = _adminService.GetAllMentorApplications().Count(a => a.Status == MentorApplicationStatus.Pending);
            return View(students);
        }

        // Instructor List
        public IActionResult Instructors()
        {
            var instructors = _adminService.GetAllInstructors();
            ViewBag.PendingCount = instructors.Count(i => i.Status == "Pending");
            return View(instructors);
        }

        // Instructor Profile/ Details
        public IActionResult Instructor_Profile()
        {
            return View();
        }

        // Reports
        public IActionResult Reports()
        {
            return View();
        }
    }
}
