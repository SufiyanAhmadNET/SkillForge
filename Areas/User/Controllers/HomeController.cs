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
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            var vm = _courseService.GetStudentDashboard(studentId);
            return View(vm);
        }

        //Profile - get
        public IActionResult Profile()
        {
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            // Use service to get comprehensive dashboard data (counts, courses, etc)
            var dvm = _courseService.GetStudentDashboard(studentId);

            // Fetch profile for additional fields not in dashboard service
            var profile = _context.StudentProfiles.FirstOrDefault(p => p.StudentId == studentId);
            if (profile != null)
            {
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
                TempData["Alert"] = "Profile created successfully!";
                TempData["AlertType"] = "success";
                return RedirectToAction("Profile");
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
                TempData["Alert"] = "Profile updated successfully.";
                TempData["AlertType"] = "success";
                return RedirectToAction("Profile");
            }
        }




        //Course Page
        [Authorize(Roles = "Student")]
        public IActionResult Courses()
        {
            var studentId = GetStudentId();
            var vm = _courseService.GetPublishedCoursePage(studentId);
            return View(vm);
        }

        // course details for students (with optional instructor preview)
        public IActionResult CourseDetails(int id, bool preview = false)
        {
            if (id <= 0)
                return RedirectToAction("Courses");

            var studentId = GetStudentId();
            
            // IF student already enrolled: Open Learning View page
            if (!preview && studentId > 0 && _enrollmentService.IsEnrolled(studentId, id))
            {
                return RedirectToAction("LearningView", new { id = id });
            }

            var vm = _courseService.GetCourseDetails(id, studentId);

            if (vm == null)
                return NotFound();

            ViewBag.IsPreview = preview;
            return View(vm);
        }

        // Learning View for enrolled students
        [Authorize(Roles = "Student")]
        public IActionResult LearningView(int id)
        {
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            if (!_enrollmentService.IsEnrolled(studentId, id))
            {
                return RedirectToAction("CourseDetails", new { id = id });
            }

            var vm = _courseService.GetCourseDetails(id, studentId);
            if (vm == null) return NotFound();

            ViewBag.CompletedLessons = _courseService.GetCompletedLessonIds(studentId, id);
            
            return View(vm);
        }

        // Mark lesson as complete - AJAX
        [HttpPost]
        [Authorize(Roles = "Student")]
        public IActionResult MarkLessonComplete(int lessonId, int courseId)
        {
            var studentId = GetStudentId();
            if (studentId == 0) return Json(new { success = false });

            bool success = _courseService.MarkLessonAsComplete(studentId, lessonId);
            
            // Recalculate progress for UI update
            var enrolledCourses = _courseService.GetEnrolledCourses(studentId);
            var course = enrolledCourses.FirstOrDefault(c => c.courseId == courseId);
            var progress = course?.ProgressPercentage ?? 0;

            return Json(new { success = success, progress = progress });
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
            
            // student details for prefill
            ViewBag.StudentEmail = result.StudentEmail;
            ViewBag.StudentMobile = result.StudentMobile;
            ViewBag.StudentName = result.StudentName;

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
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            var wishlist = _courseService.GetWishlist(studentId);
            return View(wishlist);
        }

        //Toggle Wishlist - AJAX
        [HttpPost]
        public IActionResult ToggleWishlist(int courseId)
        {
            var studentId = GetStudentId();
            if (studentId == 0) return Json(new { success = false, message = "Please login first" });

            bool added = _courseService.ToggleWishlist(studentId, courseId);
            return Json(new { success = true, added = added });
        }

        //Orders
        [Authorize(Roles = "Student")]
        public IActionResult Orders()
        {
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            var orders = _courseService.GetStudentOrders(studentId);
            return View(orders);
        }

        //Certificates
        [Authorize(Roles = "Student")]
        public IActionResult Certificates()
        {


            return View();
        }

        // ── Cart ──
        [Authorize(Roles = "Student")]
        public IActionResult Cart()
        {
            var studentId = GetStudentId();
            if (studentId == 0) return RedirectToAction("StudentLogin", "Auth");

            var cartItems = _courseService.GetCartItems(studentId);
            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int courseId)
        {
            var studentId = GetStudentId();
            if (studentId == 0) return Json(new { success = false, message = "Please login first" });

            bool added = _courseService.AddToCart(studentId, courseId);
            if (added)
            {
                var count = _courseService.GetCartCount(studentId);
                return Json(new { success = true, message = "Added to cart", cartCount = count });
            }
            return Json(new { success = false, message = "You are already enrolled in this course" });
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public IActionResult RemoveFromCart(int courseId)
        {
            var studentId = GetStudentId();
            if (studentId == 0) return Json(new { success = false, message = "Please login first" });

            _courseService.RemoveFromCart(studentId, courseId);
            var count = _courseService.GetCartCount(studentId);
            return Json(new { success = true, message = "Removed from cart", cartCount = count });
        }
    }
}
