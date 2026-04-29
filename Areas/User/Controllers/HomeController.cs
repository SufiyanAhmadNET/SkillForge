using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Services;
using SkillForge.Services.StudentService;
using System.Security.Claims;

namespace SkillForge.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : UserBaseController
    {
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
        private IConfiguration _config;
        public HomeController(CourseService courseService, EnrollmentService enrollmentService,
                              IConfiguration config)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _config = config;
        }


        //Dashboard
        [Authorize(Roles = "Student")]
        public IActionResult Dashboard()
        {

            //read Login info from Cookie
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return View(new DashboardVM { Email = email });
        }

        //Profile - get
        public IActionResult Profile()
        {

            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(id ?? string.Empty, out var studentId))
            {
                // No valid user id in claims — redirect to login or return Unauthorized
                return RedirectToAction("StudentLogin");
            }


            // Load student by primary key 
            var student = _context.Students.Find(studentId);
            if (student == null)
            {
                return NotFound();
            }
            //fetch student profile
            var profile = _context.StudentProfiles.FirstOrDefault(p => p.StudentId == student.Id);

            var dvm = new DashboardVM();
            dvm.Email = student.Email;
            if (profile != null)
            {
                //map properties
                dvm.FirstName = profile.FirstName;
                dvm.LastName = profile.LastName;
                dvm.Mobile = profile.Mobile;
                dvm.Bio = profile.Bio;
                dvm.City = profile.City;
                dvm.Profession = profile.Profession;
                dvm.PhotoPath = profile.PhotoPath ?? "/images/DefaultProfilePhoto.jfif";
            }
            return View(dvm);
        }



        [HttpPost]
        [Authorize(Roles = "Student")]
        public IActionResult Profile(StudentProfile profile, IFormFile PhotoFile)
        {
            // fetch student id from claim
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim ?? string.Empty, out var id))
            {
                return RedirectToAction("StudentLogin");
            }
            profile.StudentId = id;

            // handle photo upload
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                // create unique filename
                var fileName = Guid.NewGuid() + Path.GetExtension(PhotoFile.FileName);

                // save to wwwroot/images/profiles/
                var path = Path.Combine("wwwroot", "images", "profiles", fileName);
                //save on server
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    PhotoFile.CopyTo(stream); // write file to disk
                }

                // save path to model will goes to DB
                profile.PhotoPath = "/images/profiles/" + fileName;
            }
            else
            {
                // no new photo uploaded
                // keep existing photo from DB 
                var existing = _context.StudentProfiles.FirstOrDefault(s => s.StudentId == id);
                profile.PhotoPath = existing?.PhotoPath
                                     ?? "/images/DefaultProfilePhoto.jfif";
            }

            var existingprofile = _context.StudentProfiles.FirstOrDefault(s => s.StudentId == id);
            if (existingprofile == null)
            {     // auto model binding by ef core
                _context.Add(profile);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                existingprofile.FirstName = profile.FirstName;
                existingprofile.LastName = profile.LastName;
                existingprofile.Mobile = profile.Mobile;
                existingprofile.Bio = profile.Bio;
                existingprofile.City = profile.City;
                existingprofile.Profession = profile.Profession;
                existingprofile.PhotoPath = profile.PhotoPath;

                _context.Update(existingprofile);
                _context.SaveChanges();
                return RedirectToAction("Profile");
            }
        }




        //Course Page
        [Authorize(Roles = "Student")]
        public IActionResult Courses()
        {
            var vm = _courseService.GetPublishedCoursePage();
            return View(vm);
        }

        //Course Details Page
        public IActionResult CourseDetails(int id)
        {
            if (id <= 0)
                return RedirectToAction("Courses");

            var vm = _courseService.GetCourseDetails(id);

            if (vm == null)
                return NotFound();
            return View(vm);
        }


        //Enrolled Courses
        [Authorize(Roles = "Student")]
        public IActionResult EnrolledCourses()
        {
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            // fetch enrolled courses from service
            var enrolledCourses = _courseService.GetEnrolledCourses(studentId);
            
            return View(enrolledCourses);
        }

        //enroll in course
        public IActionResult Checkout(int courseId)
        {
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            // already enrolled? skip to success
            if (_enrollmentService.IsEnrolled(studentId, courseId))
            {
                TempData["Alert"] = "You are already enrolled in this course!";
                TempData["AlertType"] = "info";
                return RedirectToAction("EnrolledCourses", "Home");
            }

            // create razorpay order
            var result = _enrollmentService.CreateOrder(studentId, courseId);

            if (!result.Success)
            {
                TempData["Alert"] = result.Message;
                TempData["AlertType"] = "danger";
                return RedirectToAction("CourseDetails", "Home", new { id = courseId });
            }

            // pass data to checkout page
            ViewBag.RazorpayOrderId = result.RazorpayOrderId;
            ViewBag.Amount = result.Amount;          // paise
            ViewBag.AmountDisplay = result.Amount / 100m;   // INR for display
            ViewBag.CourseTitle = result.CourseTitle;
            ViewBag.CourseId = courseId;
            ViewBag.RazorpayKeyId = _config["Razorpay:KeyId"];

            return View();
        }


        // ── POST: /User/Enrollment/VerifyPayment ──
        // razorpay JS calls this after payment
        [HttpPost]
        public IActionResult VerifyPayment(string razorpay_order_id, string razorpay_payment_id, string razorpay_signature, int courseId)
        {
            var result = _enrollmentService.VerifyPayment(razorpay_order_id, razorpay_payment_id, razorpay_signature);

            if (!result.Success)
            {
                TempData["Alert"] = result.Message ?? "Payment failed.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("CourseDetails", "Home", new { id = courseId });
            }

            TempData["Alert"] = " Enrollment successful! Start learning now.";
            TempData["AlertType"] = "success";
            return RedirectToAction("EnrollmentSuccess", new { enrollmentId = result.EnrollmentId });
        }


        // ── GET: /User/Enrollment/EnrollmentSuccess ──
        public IActionResult EnrollmentSuccess(int enrollmentId)
        {
            ViewBag.EnrollmentId = enrollmentId;
            return View();
        }


        // ── helper: get student id from cookie claim ──
        private int GetStudentId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }


        //Wishlist
        [Authorize(Roles = "Student")]

        public IActionResult Wishlist()
        {
            return View();
        }

        //Orders
        [Authorize(Roles = "Student")]

        public IActionResult Orders()
        {
            return View();
        }

        //Certificates
        [Authorize(Roles = "Student")]
        public IActionResult Certificates()
        {


            return View();
        }
    }
}
