using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.Instructor.Models;
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
        public CourseReturn AddCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string action = "draft")
        {
            try
            {
                EnsureDefaultCategories();

                if (courseVM == null)
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Invalid course data." };
                }

                if (string.IsNullOrWhiteSpace(courseVM.Title))
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Course title is required." };
                }

                if (string.IsNullOrWhiteSpace(courseVM.Description))
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Course description is required." };
                }

                if (!courseVM.Duration_Weeks.HasValue || courseVM.Duration_Weeks.Value <= 0)
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Duration must be greater than 0 weeks." };
                }

                if (courseVM.Actual_Price < 0)
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Price cannot be negative." };
                }

                if (courseVM.Discount_Percent < 0 || courseVM.Discount_Percent > 100)
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Discount must be between 0 and 100." };
                }

                if (!courseVM.Difficulty.HasValue || courseVM.Difficulty.Value == Course_Difficulty.None)
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Select a valid difficulty level." };
                }

                var outcomes = courseVM.outcome?
                    .Select(o => o?.Trim())
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o!)
                    .ToList() ?? new List<string>();

                if (!outcomes.Any())
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Add at least one learning outcome." };
                }

                if (!int.TryParse(courseVM.Category_Id, out var categoryId) || categoryId <= 0)
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Please select a category." };
                }

                if (!_context.course_Categories.Any(c => c.Id == categoryId))
                {
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Selected category is not available." };
                }



                bool isSubmitAction = action?.ToLower() == "submit";

                //Course  Object
                var course = new Course
                {
                    Title = courseVM.Title,
                    instructor_id = instructorId,
                    category_id = categoryId,
                    Status = CourseStatus.Published,  // Always public -testing
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
                    Duration_Weeks = courseVM.Duration_Weeks ?? 0,
                    Difficulty = courseVM.Difficulty.Value,
                    Thumbnail_Url = SaveThumbnail(thumbnailFile),
                    Intro_Video_Url = HandleVideo(videoFile, youtubeUrl, videoType),
                };

                //Map Outcomes and make list of outcomes
                course.CourseOutcomes = outcomes
                    .Select(o => new CourseOutcomes { Outcome = o }).ToList();

                _context.Courses.Add(course);
                _context.SaveChanges();

                return new CourseReturn
                {
                    Success = true,
                    message = isSubmitAction ? CourseMessage.SentForApproval : CourseMessage.SavedToDraft,
                    courseData = course
                };
            }
            catch (Exception ex)
            {
                return new CourseReturn
                {
                    Success = false,
                    message = CourseMessage.CourseNotAdded,
                    TechnicalMessage = ex.Message
                };
            }
        } //Add Course



        //========================
        //View Course List-  instructor 
        //========================
        public List<MyCourseVM> MyCourses(int instructorId)
        {
            return _context.Courses
                .Where(c => c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .Select(c => new MyCourseVM
                {
                    CourseId = c.Id,
                    Thumbnail_Url = c.CourseDetails.Thumbnail_Url,
                    Status = c.Status,
                    CategoryName = c.courseCategory != null ? c.courseCategory.Name : "Uncategorized",
                    Title = c.Title,
                    Total_Price = c.CourseDetails.Total_Price

                }).ToList();
        }

        //========================
        // Build CoursePageVM for student View
        //========================
        public CoursePageVM GetPublishedCoursePage()
        {
            var published = _context.Courses
                .Where(c => c.Status == CourseStatus.Published || c.Status == CourseStatus.Approved || c.Status == CourseStatus.PendingReview)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .ToList();

            var popular = published
                .OrderByDescending(c => c.Id)
                .Take(8)
                .Select(c => new CourseCardVM
                {
                    Title = c.Title,
                    SubTitle = c.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                    CategoryName = c.courseCategory?.Name ?? "Uncategorized",
                    Difficulty = c.CourseDetails?.Difficulty.ToString() ?? "None",
                    Total_Price = c.CourseDetails?.Total_Price ?? 0,
                    Actual_Price = c.CourseDetails?.Actual_Price ?? 0,
                    Discount_Percent = c.CourseDetails != null ? c.CourseDetails.Discount_Percent : 0,
                    Thumbnail_Url = c.CourseDetails?.Thumbnail_Url,
                    courseId = c.Id
                })
                .ToList();

            var categories = published
                .GroupBy(c => c.courseCategory?.Name ?? "Uncategorized")
                .Select(g => new CategorySectionVM
                {
                    CategoryName = g.Key,
                    Courses = g.Select(c => new CourseCardVM
                    {
                        Title = c.Title,
                        SubTitle = c.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                        CategoryName = c.courseCategory?.Name ?? "Uncategorized",
                        Difficulty = c.CourseDetails?.Difficulty.ToString() ?? "None",
                        Total_Price = c.CourseDetails?.Total_Price ?? 0,
                        Actual_Price = c.CourseDetails?.Actual_Price ?? 0,
                        Discount_Percent = c.CourseDetails != null ? c.CourseDetails.Discount_Percent : 0,
                        Thumbnail_Url = c.CourseDetails?.Thumbnail_Url,
                        courseId = c.Id
                    }).ToList()
                })
                .ToList();

            return new CoursePageVM
            {
                PopularCourses = popular,
                CategorySections = categories
            };
        }

        //========================
        //View Course 
        //========================
        public CourseDetailsVM? GetCourseDetails(int courseId)
        {
            // fetch course and related data in one query
            var course = _context.Courses
                .Where(c => c.Id == courseId)
                .Include(c => c.CourseDetails)
                .Include(c => c.CourseOutcomes)
                .Include(c => c.courseCategory)
                .FirstOrDefault();

            if (course == null) return null;

            return new CourseDetailsVM
            {
                CourseId = course.Id,
                Title = course.Title,
                Desciption = course.CourseDetails?.Description,
                VideoUrl = course.CourseDetails?.Intro_Video_Url,
                ActualPrice = course.CourseDetails?.Actual_Price ?? 0,
                TotalPrice = course.CourseDetails?.Total_Price ?? 0,
                DiscountPercent = (float)(course.CourseDetails?.Discount_Percent ?? 0),
                outcomes = course.CourseOutcomes?.ToList() ?? new List<CourseOutcomes>(),
                SubTitle = course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty
            };
        }  //View Course 


        // Fetch instructor course details with enrolled students
        public CourseDetailsVM? GetInstructorCourseDetails(int courseId, int instructorId)
        {
            var course = _context.Courses
                .Where(c => c.Id == courseId && c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .Include(c => c.CourseOutcomes)
                .Include(c => c.courseCategory)
                .FirstOrDefault();

            if (course == null) return null;

            return new CourseDetailsVM
            {
                CourseId = course.Id,
                Title = course.Title,
                Desciption = course.CourseDetails?.Description,
                VideoUrl = course.CourseDetails?.Intro_Video_Url,
                ActualPrice = course.CourseDetails?.Actual_Price ?? 0,
                TotalPrice = course.CourseDetails?.Total_Price ?? 0,
                DiscountPercent = (float)(course.CourseDetails?.Discount_Percent ?? 0),
                outcomes = course.CourseOutcomes?.ToList() ?? new List<CourseOutcomes>(),
                SubTitle = course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                Status = course.Status.ToString(),
                Duration = course.CourseDetails?.Duration_Weeks ?? 0,
                Difficulty = course.CourseDetails?.Difficulty.ToString(),
                CategoryName = course.courseCategory?.Name,
                ThumbnailUrl = course.CourseDetails?.Thumbnail_Url
            };
        }// Fetch instructor course


        // Delete course -instructor
        public bool DeleteCourse(int courseId, int instructorId)
        {
            try
            {
                var course = _context.Courses
                    .Where(c => c.Id == courseId && c.instructor_id == instructorId)
                    .Include(c => c.CourseDetails)
                    .Include(c => c.CourseOutcomes)
                    .FirstOrDefault();

                if (course == null)
                    return false;

                // Remove related data
                if (course.CourseDetails != null)
                    _context.CourseDetails.Remove(course.CourseDetails);

                if (course.CourseOutcomes != null)
                    _context.CourseOutcomes.RemoveRange(course.CourseOutcomes);

                _context.Courses.Remove(course);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }// Delete course -instructor


        // SaveThumbnail                       
        private string? SaveThumbnail(IFormFile file)
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
            string path = Path.Combine(_env.WebRootPath, "uploads", "thumbnails");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

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


        //============
        //save video 
        private string HandleVideo(IFormFile file, string youtubeUrl, string videoType)
        {
            var normalizedVideoType = (videoType ?? string.Empty).Trim().ToLower();

            if (string.IsNullOrWhiteSpace(normalizedVideoType))
            {
                normalizedVideoType = !string.IsNullOrWhiteSpace(youtubeUrl) ? "youtube" : "upload";
            }

            if (normalizedVideoType == "youtube")
            {
                if (string.IsNullOrWhiteSpace(youtubeUrl))
                {
                    throw new Exception("Please provide a YouTube intro video link.");
                }

                if (!Uri.TryCreate(youtubeUrl.Trim(), UriKind.Absolute, out var parsedUri))
                {
                    throw new Exception("Invalid YouTube URL.");
                }

                var host = parsedUri.Host.ToLowerInvariant();
                if (!host.Contains("youtube.com") && !host.Contains("youtu.be"))
                {
                    throw new Exception("Only YouTube links are allowed for this option.");
                }

                return youtubeUrl.Trim();
            }

            if (normalizedVideoType == "upload")
            {
                if (file == null || file.Length == 0)
                {
                    throw new Exception("Please upload an intro video file.");
                }

                long maxSize = 50 * 1024 * 1024; // 50MB
                var allowedExtensions = new[] { ".mp4", ".webm", ".mov" };

                // size check
                if (file.Length > maxSize)
                    throw new Exception("Video must be less than 50MB");

                // extension check
                string ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                    throw new Exception("Only mp4, webm, mov allowed");

                // unique filename
                string fileName = Guid.NewGuid().ToString() + ext;

                // path
                string path = Path.Combine(_env.WebRootPath, "uploads", "videos");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fullPath = Path.Combine(path, fileName);

                // save file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // return saved path
                return "/uploads/videos/" + fileName;
            }

            throw new Exception("Please choose a valid video source.");
        }//save video

        private void EnsureDefaultCategories()
        {
            if (_context.course_Categories.Any())
            {
                return;
            }

            var defaultCategories = new[]
            {
                "Software Development",
                "Data Science",
                "AI / Machine Learning",
                "DevOps & Cloud",
                "Cybersecurity"
            };

            _context.course_Categories.AddRange(defaultCategories.Select(name => new Course_Category { Name = name }));
            _context.SaveChanges();
        }

        // show enrolled courses for a student
        public List<CourseCardVM> GetEnrolledCourses(int studentId)
        {
            // Materialize the query results     
            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(e => e.Course)
                    .ThenInclude(c => c.courseCategory)
                .ToList();

            return enrollments
                .Select(e => new CourseCardVM
                {
                    courseId = e.Course.Id,
                    Title = e.Course.Title,
                    SubTitle = e.Course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                    CategoryName = e.Course.courseCategory?.Name ?? "Uncategorized",
                    Difficulty = e.Course.CourseDetails?.Difficulty.ToString() ?? "None",
                    Total_Price = e.Course.CourseDetails?.Total_Price ?? 0,
                    Actual_Price = e.Course.CourseDetails?.Actual_Price ?? 0,
                    Discount_Percent = e.Course.CourseDetails?.Discount_Percent ?? 0,
                    Thumbnail_Url = e.Course.CourseDetails?.Thumbnail_Url,
                   // Progress = 0
                })
                .ToList();
        }

     
    }
}