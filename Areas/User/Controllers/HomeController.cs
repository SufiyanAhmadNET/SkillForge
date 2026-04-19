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
       // private readonly StudentService _studentService;
        
        //public HomeController (StudentService studentService)
        //{
        //    _studentService = studentService;
        //}

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
            //var studentid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            //var course = _studentService.GetCoursePage(studentid);


            var vm = new CoursePageVM
            {
                PopularCourses = new List<CourseCardVM>
        {
            new CourseCardVM
            {
                Title = "C# Basics",
                SubTitle = "Learn fundamentals",
                CategoryName = "Programming",
                Difficulty = "Beginner",
                Total_Price = 499,
                Actual_Price = 999,
                Discount_Percent = 50,
                Thumbnail_Url = "https://via.placeholder.com/300"
                
            }
        },

                CategorySections = new List<CategorySectionVM>
        {
            new CategorySectionVM
            {
                CategoryName = "A.I",
                Courses = new List<CourseCardVM>
                {
                    new CourseCardVM
                    {
                        Title = "Machine Learning",
                        SubTitle = "Deep dive into Concepts of Machine Learning , Build Real Projects",
                        CategoryName = "A.I",
                        Difficulty = "Advanced",
                        Total_Price = 799,
                        Actual_Price = 1499,
                        Discount_Percent = 47,
                        Thumbnail_Url = "https://images.unsplash.com/photo-1619314383191-3d75d5e26a7f?w=600&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTh8fHlvdXR1YmUlMjB0aHVtYm5haWx8ZW58MHx8MHx8fDA%3D"
                    },
                    new CourseCardVM
                    {
                        Title = "ASP.NET Core",
                        SubTitle = "Build web apps",
                        CategoryName = "Programming",
                        Difficulty = "Intermediate",
                        Total_Price = 699,
                        Actual_Price = 1299,
                        Discount_Percent = 46,
                        Thumbnail_Url = "https://www.google.com/imgres?q=youtube%20thumbnail%20for%20educational%20video&imgurl=https%3A%2F%2Fimg.freepik.com%2Fpremium-psd%2Fschool-education-admission-youtube-thumbnail-web-banner-template_475351-436.jpg&imgrefurl=https%3A%2F%2Fwww.freepik.com%2Fpremium-psd%2Fschool-education-admission-youtube-thumbnail-web-banner-template_45155853.htm&docid=uUi9UMPQ_bI7eM&tbnid=WaHpIDmWfPH05M&vet=12ahUKEwj854at8u-TAxX1c_UHHbd3M64QnPAOegQIHhAB..i&w=626&h=352&hcb=2&ved=2ahUKEwj854at8u-TAxX1c_UHHbd3M64QnPAOegQIHhAB"
                    }
                }
            },

            new CategorySectionVM
            {
                CategoryName = "Design",
                Courses = new List<CourseCardVM>() // empty 
            }
        }
            };

            return View(vm);
        }

         
        




        public IActionResult CourseDetails()
        {
            return View();
        }
        //Enrolled Courses
        [Authorize(Roles = "Student")]

        public IActionResult EnrolledCourses()
        {
            return View();
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
