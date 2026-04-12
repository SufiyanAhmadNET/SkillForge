using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkillForge.Data;
using SkillForge.Models;

namespace SkillForge.Services
{
    public class CourseService
    {
        private readonly SkillForgeDbContext _context;
        private readonly IWebHostEnvironment _env;

        //Constructor
        public CourseService(SkillForgeDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        //############################
        //Add Course Method
        public CourseReturn AddCourse(CourseVM courseVM, int Instructorid, IFormFile thumbnailFile)
        {
            //  Validation
            if (courseVM == null || string.IsNullOrWhiteSpace(courseVM.Title) ||
                courseVM.Actual_Price <= 0 || courseVM.Duration_Weeks <= 0 ||
                courseVM.outcome == null || !courseVM.outcome.Any())
            {
                return new CourseReturn { Success = false, message = CourseMessage.EmptyFields };
            }


         
            //Course  Object
            var course = new Course
            {
                Title = courseVM.Title,
                instructor_id = Instructorid, //from controller
                category_id = courseVM.category_id,
                Status = CourseStatus.Draft,
                Rejection_Reason = null
            };

            //CourseDetails 
            course.CourseDetails = new CourseDetails
            {
                Description = courseVM.Description,
                Actual_Price = courseVM.Actual_Price,
                Discount_Percent = courseVM.Discount_Percent,
                Total_Price = courseVM.Actual_Price -
                              (courseVM.Actual_Price * courseVM.Discount_Percent / 100),
                Duration_Weeks = courseVM.Duration_Weeks,
                Difficulty = (Course_Difficulty)courseVM.Difficulty,
            };

            if(thumbnailFile != null)
            { try
                {
                    courseVM.Thumbnail_Url = SaveThumbnail(thumbnailFile);
                }
                catch (Exception ex)
                {
                    return new CourseReturn {message = CourseMessage.thumbnailNotUpload };
                  }         
               
            }
                      
            //Map Outcomes ,make list of outcomes
            course.CourseOutcomes = courseVM.outcome
                .Select(o => new CourseOutcomes { Outcome = o }).ToList();
         
            _context.Courses.Add(course);           
            _context.SaveChanges();
              
            return new CourseReturn { Success = true, message = CourseMessage.CourseAdded };
        }


        // SaveThumbnail                       
        private string SaveThumbnail(IFormFile file)
        {
            // image limits
            long maxSize = 2 * 1024 * 1024; // 2MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            // if file exists
            if (file == null || file.Length == 0)
            {
                return null; // No file uploaded
            }

            //  file size
            if (file.Length > maxSize)
            {
                throw new Exception("File size must be less than 2MB");
            }

            //file extension
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Only .jpg, .jpeg, and .png files are allowed");
            }

            //  generate filename 
            string fileName = Guid.NewGuid().ToString() + fileExtension;

            //  save file
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnails");
           
            //path to save file
            string fullPath = Path.Combine(path, fileName);

            //Save  file to steam
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Return t path to store in database
            return "/uploads/thumbnails/" + fileName;
        }

        //save video 

    }
}
