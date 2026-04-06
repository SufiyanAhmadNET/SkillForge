using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.User.Models;
using System.Security.Claims;
using SkillForge.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkillForge.Data;

namespace SkillForge.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : UserBaseController
    {
        // Use the DbContext provided by UserBaseController via property injection

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
            var profile = _context.StudentProfiles.FirstOrDefault(p=>p.StudentId == student.Id);

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
                // keep existing photo from DB — dont overwrite with null
                var existing = _context.StudentProfiles.FirstOrDefault(s => s.StudentId == id);
               profile.PhotoPath = existing?.PhotoPath
                                    ?? "/images/DefaultProfilePhoto.jfif";
            }
         
            var existingprofile = _context.StudentProfiles.FirstOrDefault(s => s.StudentId == id);
            if (existingprofile == null)
            {
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
