using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Models;

namespace SkillForge.Services
{
    public class CourseService
    {
        private readonly SkillForgeDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Constructor
        public CourseService(SkillForgeDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        //############################
        // Add Course
        public CourseReturn AddCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string action = "draft")
        {
            try
            {
                EnsureDefaultCategories();

                if (courseVM == null)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Invalid course data." };

                if (string.IsNullOrWhiteSpace(courseVM.Title))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Course title is required." };

                if (string.IsNullOrWhiteSpace(courseVM.Description))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Course description is required." };

                if (!courseVM.Duration_Weeks.HasValue || courseVM.Duration_Weeks.Value <= 0)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Duration must be greater than 0 weeks." };

                if (courseVM.Actual_Price < 0)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Price cannot be negative." };

                if (courseVM.Discount_Percent < 0 || courseVM.Discount_Percent > 100)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Discount must be between 0 and 100." };

                if (!courseVM.Difficulty.HasValue || courseVM.Difficulty.Value == Course_Difficulty.None)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Select a valid difficulty level." };

                var outcomes = courseVM.outcome?
                    .Select(o => o?.Trim())
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o!)
                    .ToList() ?? new List<string>();

                if (!outcomes.Any())
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Add at least one learning outcome." };

                if (!int.TryParse(courseVM.Category_Id, out var categoryId) || categoryId <= 0)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Please select a category." };

                if (!_context.course_Categories.Any(c => c.Id == categoryId))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Selected category is not available." };


                bool isSubmitAction = action?.ToLower() == "submit";

                // create course object
                var course = new Course
                {
                    Title = courseVM.Title,
                    instructor_id = instructorId,
                    category_id = categoryId,
                    Status = CourseStatus.Published, // Testing mode
                    Rejection_Reason = null
                };

                // course details
                course.CourseDetails = new CourseDetails
                {
                    Description = courseVM.Description,
                    Actual_Price = courseVM.Actual_Price,
                    Discount_Percent = courseVM.Discount_Percent,
                    Total_Price = courseVM.Actual_Price - (courseVM.Actual_Price * courseVM.Discount_Percent / 100),
                    Duration_Weeks = courseVM.Duration_Weeks ?? 0,
                    Difficulty = courseVM.Difficulty.Value,
                    Thumbnail_Url = SaveThumbnail(thumbnailFile),
                    Intro_Video_Url = HandleVideo(videoFile, youtubeUrl, videoType),
                };

                // map outcomes
                course.CourseOutcomes = outcomes
                    .Select(o => new CourseOutcomes { Outcome = o }).ToList();

                _context.Courses.Add(course);
                _context.SaveChanges();

                // save syllabus 
                if (courseVM.Syllabus != null && courseVM.Syllabus.Any())
                {
                    foreach (var modVM in courseVM.Syllabus)
                    {
                        if (string.IsNullOrWhiteSpace(modVM.ModuleName)) continue;

                        var module = new CourseModules
                        {
                            CourseId = course.Id,
                            ModuleName = modVM.ModuleName
                        };
                        _context.CourseModules.Add(module);
                        _context.SaveChanges();

                        if (modVM.Lessons != null && modVM.Lessons.Any())
                        {
                            int lessonOrder = 1;
                            foreach (var lesVM in modVM.Lessons)
                            {
                                if (string.IsNullOrWhiteSpace(lesVM.Title)) continue;

                                var lesson = new CourseLesson
                                {
                                    ModuleId = module.Id,
                                    Title = lesVM.Title,
                                    VideoUrl = lesVM.VideoUrl,
                                    Order = lessonOrder++
                                };
                                _context.CourseLessons.Add(lesson);
                            }
                        }
                    }
                    _context.SaveChanges();
                }

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
        }


        //############################
        // Update Course
        public CourseReturn UpdateCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string action = "draft")
        {
            try
            {
                var course = _context.Courses
                    .Where(c => c.Id == courseVM.Id && c.instructor_id == instructorId)
                    .Include(c => c.CourseDetails)
                    .Include(c => c.CourseOutcomes)
                    .FirstOrDefault();

                if (course == null)
                    return new CourseReturn { Success = false, message = CourseMessage.CourseNotAdded, TechnicalMessage = "Course not found." };

                // update basic info
                course.Title = courseVM.Title;
                course.category_id = int.Parse(courseVM.Category_Id ?? "0");
                
                if (action?.ToLower() == "submit")
                    course.Status = CourseStatus.Published;

                if (course.CourseDetails == null) course.CourseDetails = new CourseDetails();

                course.CourseDetails.Description = courseVM.Description;
                course.CourseDetails.Actual_Price = courseVM.Actual_Price;
                course.CourseDetails.Discount_Percent = courseVM.Discount_Percent;
                course.CourseDetails.Total_Price = courseVM.Actual_Price - (courseVM.Actual_Price * courseVM.Discount_Percent / 100);
                course.CourseDetails.Duration_Weeks = courseVM.Duration_Weeks ?? 0;
                course.CourseDetails.Difficulty = courseVM.Difficulty ?? Course_Difficulty.Beginner;

                // handle files
                if (thumbnailFile != null)
                    course.CourseDetails.Thumbnail_Url = SaveThumbnail(thumbnailFile);
                
                if (videoFile != null || !string.IsNullOrWhiteSpace(youtubeUrl))
                    course.CourseDetails.Intro_Video_Url = HandleVideo(videoFile, youtubeUrl, videoType);

                // update outcomes
                if (courseVM.outcome != null)
                {
                    _context.CourseOutcomes.RemoveRange(course.CourseOutcomes);
                    course.CourseOutcomes = courseVM.outcome
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => new CourseOutcomes { Outcome = o.Trim() }).ToList();
                }

                // update syllabus
                if (courseVM.Syllabus != null)
                {
                    var existingModules = _context.CourseModules.Where(m => m.CourseId == course.Id).ToList();
                    _context.CourseModules.RemoveRange(existingModules);
                    _context.SaveChanges();

                    foreach (var modVM in courseVM.Syllabus)
                    {
                        if (string.IsNullOrWhiteSpace(modVM.ModuleName)) continue;

                        var module = new CourseModules
                        {
                            CourseId = course.Id,
                            ModuleName = modVM.ModuleName
                        };
                        _context.CourseModules.Add(module);
                        _context.SaveChanges();

                        if (modVM.Lessons != null)
                        {
                            int lessonOrder = 1;
                            foreach (var lesVM in modVM.Lessons)
                            {
                                if (string.IsNullOrWhiteSpace(lesVM.Title)) continue;

                                var lesson = new CourseLesson
                                {
                                    ModuleId = module.Id,
                                    Title = lesVM.Title,
                                    VideoUrl = lesVM.VideoUrl,
                                    Order = lessonOrder++
                                };
                                _context.CourseLessons.Add(lesson);
                            }
                        }
                    }
                }

                _context.Courses.Update(course);
                _context.SaveChanges();

                return new CourseReturn { Success = true, message = CourseMessage.SavedToDraft };
            }
            catch (Exception ex)
            {
                return new CourseReturn { Success = false, message = CourseMessage.CourseNotAdded, TechnicalMessage = ex.Message };
            }
        }


        //############################
        // Instructor Courses
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
        public CoursePageVM GetPublishedCoursePage(int studentId = 0)
        {
            var published = _context.Courses
                .Where(c => c.Status == CourseStatus.Published || c.Status == CourseStatus.Approved || c.Status == CourseStatus.PendingReview)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .ToList();

            var wishlistedIds = studentId > 0 
                ? _context.Wishlists.Where(w => w.StudentId == studentId).Select(w => w.CourseId).ToList() 
                : new List<int>();

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
                    courseId = c.Id,
                    IsWishListed = wishlistedIds.Contains(c.Id)
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
                        courseId = c.Id,
                        IsWishListed = wishlistedIds.Contains(c.Id)
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
        public CourseDetailsVM? GetCourseDetails(int courseId, int studentId = 0)
        {
            // fetch course and related data in one query
            var course = _context.Courses
                .Where(c => c.Id == courseId)
                .Include(c => c.CourseDetails)
                .Include(c => c.CourseOutcomes)
                .Include(c => c.courseCategory)
                .FirstOrDefault();

            if (course == null) return null;

            // Fetch modules and lessons
            var modules = _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Lessons)
                .OrderBy(m => m.Id)
                .ToList();

            bool isWishlisted = false;
            if (studentId > 0)
            {
                isWishlisted = _context.Wishlists.Any(w => w.StudentId == studentId && w.CourseId == courseId);
            }

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
                IsWishlisted = isWishlisted,
                modules = modules,
                Duration = course.CourseDetails?.Duration_Weeks ?? 0,
                Difficulty = course.CourseDetails?.Difficulty.ToString(),
                ThumbnailUrl = course.CourseDetails?.Thumbnail_Url,
                CategoryName = course.courseCategory?.Name
            };
        }  //View Course 


        //========================
        // Syllabus Management
        //========================

        public bool AddModule(int courseId, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return false;

            var module = new CourseModules
            {
                CourseId = courseId,
                ModuleName = moduleName
            };
            _context.CourseModules.Add(module);
            return _context.SaveChanges() > 0;
        }

        public bool AddLesson(int moduleId, string title, string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            var lesson = new CourseLesson
            {
                ModuleId = moduleId,
                Title = title,
                VideoUrl = videoUrl,
                Order = _context.CourseLessons.Count(l => l.ModuleId == moduleId) + 1
            };
            _context.CourseLessons.Add(lesson);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteModule(int moduleId)
        {
            var module = _context.CourseModules.Find(moduleId);
            if (module == null) return false;

            _context.CourseModules.Remove(module);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteLesson(int lessonId)
        {
            var lesson = _context.CourseLessons.Find(lessonId);
            if (lesson == null) return false;

            _context.CourseLessons.Remove(lesson);
            return _context.SaveChanges() > 0;
        }

        //========================
        // Progress Tracking
        //========================

        public bool MarkLessonAsComplete(int studentId, int lessonId)
        {
            var existing = _context.UserProgress
                .FirstOrDefault(p => p.StudentId == studentId && p.LessonId == lessonId);

            if (existing != null)
            {
                existing.IsCompleted = !existing.IsCompleted;
                _context.UserProgress.Update(existing);
            }
            else
            {
                _context.UserProgress.Add(new UserLessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = true
                });
            }
            return _context.SaveChanges() > 0;
        }

        public List<int> GetCompletedLessonIds(int studentId, int courseId)
        {
            var lessonIds = _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .SelectMany(m => m.Lessons)
                .Select(l => l.Id)
                .ToList();

            return _context.UserProgress
                .Where(p => p.StudentId == studentId && p.IsCompleted && lessonIds.Contains(p.LessonId))
                .Select(p => p.LessonId)
                .ToList();
        }


        // get course data for editing
        public CourseVM? GetCourseForEdit(int courseId, int instructorId)
        {
            var course = _context.Courses
                .Where(c => c.Id == courseId && c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .Include(c => c.CourseOutcomes)
                .FirstOrDefault();

            if (course == null) return null;

            var syllabus = _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Lessons)
                .OrderBy(m => m.Id)
                .Select(m => new ModuleVM
                {
                    ModuleName = m.ModuleName,
                    Lessons = m.Lessons.OrderBy(l => l.Order).Select(l => new LessonVM
                    {
                        Title = l.Title,
                        VideoUrl = l.VideoUrl
                    }).ToList()
                }).ToList();

            return new CourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.CourseDetails?.Description ?? string.Empty,
                Category_Id = course.category_id.ToString(),
                Actual_Price = course.CourseDetails?.Actual_Price ?? 0,
                Discount_Percent = course.CourseDetails?.Discount_Percent ?? 0,
                Total_Price = course.CourseDetails?.Total_Price ?? 0,
                Difficulty = course.CourseDetails?.Difficulty,
                Duration_Weeks = course.CourseDetails?.Duration_Weeks,
                Thumbnail_Url = course.CourseDetails?.Thumbnail_Url,
                Intro_Video_Url = course.CourseDetails?.Intro_Video_Url,
                outcome = course.CourseOutcomes?.Select(o => o.Outcome).ToList(),
                CourseStatus = course.Status,
                Syllabus = syllabus
            };
        }

        // get comprehensive dashboard data for instructor
        public InstructorDashboardVM GetInstructorDashboard(int instructorId)
        {
            var instructor = _context.instructors
                .Include(i => i.Profile)
                .FirstOrDefault(i => i.Id == instructorId);

            var courses = _context.Courses
                .Where(c => c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .ToList();

            var courseIds = courses.Select(c => c.Id).ToList();

            var enrollments = _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToList();

            var earnings = _context.Payments
                .Where(p => courseIds.Contains(p.Enrollment.CourseId) && p.Status == PaymentStatus.Success)
                .Sum(p => p.Amount);

            var recentEnrollments = _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Student)
                .ThenInclude(s => s.Profile)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .Take(5)
                .ToList();

            var vm = new InstructorDashboardVM
            {
                FirstName = instructor?.Profile?.FirstName ?? "Instructor",
                LastName = instructor?.Profile?.LastName,
                PhotoPath = instructor?.Profile?.PhotoPath ?? "/images/DefaultProfilePhoto.jfif",
                TotalCourses = courses.Count,
                TotalStudents = enrollments.Count,
                TotalEarnings = earnings,
                AvgRating = 5.0, // static rating as requested
                ActiveCourses = courses.Select(c => new CourseStatsVM
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    StudentCount = enrollments.Count(e => e.CourseId == c.Id),
                    Rating = 5.0 // static rating
                }).Take(5).ToList(),
                RecentEnrollments = recentEnrollments.Select(e => new RecentEnrollmentVM
                {
                    StudentName = e.Student.Profile != null ? $"{e.Student.Profile.FirstName} {e.Student.Profile.LastName}".Trim() : e.Student.Email.Split('@')[0],
                    CourseTitle = e.Course.Title,
                    EnrolledDate = e.EnrolledAt.ToString("MMM dd"),
                    Initial = (e.Student.Profile?.FirstName ?? e.Student.Email).Substring(0, 1).ToUpper()
                }).ToList()
            };

            return vm;
        }

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

            // fetch modules/lessons for progress calculation
            var courseLessons = _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .SelectMany(m => m.Lessons)
                .ToList();
            var totalLessons = courseLessons.Count;

            // fetch enrolled students
            var enrollments = _context.Enrollments
                .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Student)
                .ThenInclude(s => s.Profile)
                .ToList();

            var studentIds = enrollments.Select(e => e.StudentId).ToList();
            var allProgress = _context.UserProgress
                .Where(p => studentIds.Contains(p.StudentId) && p.IsCompleted)
                .ToList();

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
                ThumbnailUrl = course.CourseDetails?.Thumbnail_Url,
                modules = _context.CourseModules.Where(m => m.CourseId == courseId).Include(m => m.Lessons).ToList(),
                EnrolledStudents = enrollments.Select(e => {
                    var studentCompleted = allProgress.Count(p => p.StudentId == e.StudentId && courseLessons.Any(cl => cl.Id == p.LessonId));
                    var progress = totalLessons > 0 ? (int)((float)studentCompleted / totalLessons * 100) : 0;
                    
                    return new StudentEnrollmentVM
                    {
                        StudentId = e.StudentId,
                        Name = e.Student.Profile?.FirstName ?? e.Student.Email.Split('@')[0],
                        Email = e.Student.Email,
                        EnrolledAt = e.EnrolledAt.ToString("MMM dd, yyyy"),
                        PhotoPath = e.Student.Profile?.PhotoPath ?? "/images/DefaultProfilePhoto.jfif",
                        Initial = (e.Student.Profile?.FirstName ?? e.Student.Email).Substring(0, 1).ToUpper(),
                        Progress = progress // Added this to VM
                    };
                }).ToList()
            };
        }
// Fetch instructor course


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

            var courseIds = enrollments.Select(e => e.Course.Id).ToList();

            // Get all lessons for these courses to calculate progress
            var modules = _context.CourseModules
                .Where(m => courseIds.Contains(m.CourseId))
                .Include(m => m.Lessons)
                .ToList();

            var progress = _context.UserProgress
                .Where(p => p.StudentId == studentId && p.IsCompleted)
                .ToList();

            return enrollments
                .Select(e => {
                    var courseLessons = modules.Where(m => m.CourseId == e.Course.Id).SelectMany(m => m.Lessons).ToList();
                    var totalLessons = courseLessons.Count;
                    var completedLessons = progress.Count(p => courseLessons.Any(cl => cl.Id == p.LessonId));
                    var progressPercentage = totalLessons > 0 ? (int)((float)completedLessons / totalLessons * 100) : 0;

                    return new CourseCardVM
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
                        ProgressPercentage = progressPercentage
                    };
                })
                .ToList();
        }

        // Toggle course in wishlist
        public bool ToggleWishlist(int studentId, int courseId)
        {
            var existing = _context.Wishlists
                .FirstOrDefault(w => w.StudentId == studentId && w.CourseId == courseId);

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                _context.SaveChanges();
                return false; // Removed
            }
            else
            {
                _context.Wishlists.Add(new Wishlist
                {
                    StudentId = studentId,
                    CourseId = courseId
                });
                _context.SaveChanges();
                return true; // Added
            }
        }

        // Get student's wishlist
        public List<CourseCardVM> GetWishlist(int studentId)
        {
            var wishlist = _context.Wishlists
                .Where(w => w.StudentId == studentId)
                .Include(w => w.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(w => w.Course)
                    .ThenInclude(c => c.courseCategory)
                .ToList();

            return wishlist.Select(w => new CourseCardVM
            {
                courseId = w.Course.Id,
                Title = w.Course.Title,
                SubTitle = w.Course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                CategoryName = w.Course.courseCategory?.Name ?? "Uncategorized",
                Difficulty = w.Course.CourseDetails?.Difficulty.ToString() ?? "None",
                Total_Price = w.Course.CourseDetails?.Total_Price ?? 0,
                Actual_Price = w.Course.CourseDetails?.Actual_Price ?? 0,
                Discount_Percent = w.Course.CourseDetails?.Discount_Percent ?? 0,
                Thumbnail_Url = w.Course.CourseDetails?.Thumbnail_Url,
                IsWishListed = true
            }).ToList();
        }

        // get comprehensive dashboard data for student
        public DashboardVM GetStudentDashboard(int studentId)
        {
            var student = _context.Students
                .Include(s => s.Profile)
                .FirstOrDefault(s => s.Id == studentId);

            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(e => e.Course)
                    .ThenInclude(c => c.courseCategory)
                .ToList();

            var wishlistCount = _context.Wishlists.Count(w => w.StudentId == studentId);

            var recommended = _context.Courses
                .Where(c => c.Status == CourseStatus.Published)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .OrderBy(r => Guid.NewGuid()) // random recommendation
                .Take(3)
                .Select(c => new CourseCardVM
                {
                    courseId = c.Id,
                    Title = c.Title,
                    CategoryName = c.courseCategory.Name,
                    Total_Price = c.CourseDetails.Total_Price,
                    Thumbnail_Url = c.CourseDetails.Thumbnail_Url
                }).ToList();

            return new DashboardVM
            {
                Id = student?.Id ?? 0,
                Email = student?.Email,
                FirstName = student?.Profile?.FirstName ?? "Student",
                LastName = student?.Profile?.LastName,
                Mobile = student?.Profile?.Mobile,
                Bio = student?.Profile?.Bio,
                City = student?.Profile?.City,
                Profession = student?.Profile?.Profession,
                PhotoPath = student?.Profile?.PhotoPath ?? "/images/DefaultProfilePhoto.jfif",
                EnrolledCount = enrollments.Count,
                WishlistCount = wishlistCount,
                CertificateCount = 0, // Placeholder for now
                CompletedCount = 0,   // Placeholder for now
                EnrolledCourses = enrollments.Select(e => new CourseCardVM
                {
                    courseId = e.Course.Id,
                    Title = e.Course.Title,
                    Thumbnail_Url = e.Course.CourseDetails?.Thumbnail_Url,
                    SubTitle = e.Course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? ""
                }).Take(4).ToList(),
                RecommendedCourses = recommended
            };
        }

        // fetch student order history
        public OrderHistoryVM GetStudentOrders(int studentId)
        {
            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(e => e.Payment)
                .OrderByDescending(e => e.EnrolledAt)
                .ToList();

            var orders = enrollments.Select(e => new StudentOrderVM
            {
                OrderId = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                ThumbnailUrl = e.Course.CourseDetails?.Thumbnail_Url ?? string.Empty,
                Amount = e.Payment?.Amount ?? 0,
                OrderDate = e.EnrolledAt,
                PaymentStatus = e.Payment?.Status.ToString() ?? "Pending",
                RazorpayOrderId = e.Payment?.RazorpayOrderId ?? "N/A"
            }).ToList();

            return new OrderHistoryVM
            {
                Orders = orders,
                TotalCourses = orders.Count(o => o.PaymentStatus == "Success"),
                TotalSpent = orders.Where(o => o.PaymentStatus == "Success").Sum(o => o.Amount),
                TotalSaved = enrollments
                    .Where(e => e.Payment?.Status == PaymentStatus.Success)
                    .Sum(e => (e.Course.CourseDetails?.Actual_Price ?? 0) - (e.Payment?.Amount ?? 0))
            };
        }

        // ── Cart Methods ──
        public bool AddToCart(int studentId, int courseId)
        {
            var exists = _context.Carts.Any(c => c.StudentId == studentId && c.CourseId == courseId);
            if (exists) return true;

            var enrolled = _context.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
            if (enrolled) return false;

            _context.Carts.Add(new Cart { StudentId = studentId, CourseId = courseId });
            _context.SaveChanges();
            return true;
        }

        public List<CourseCardVM> GetCartItems(int studentId)
        {
            return _context.Carts
                .Where(c => c.StudentId == studentId)
                .Include(c => c.Course)
                    .ThenInclude(co => co.CourseDetails)
                .Include(c => c.Course)
                    .ThenInclude(co => co.courseCategory)
                .Select(c => new CourseCardVM
                {
                    courseId = c.CourseId,
                    Title = c.Course.Title,
                    SubTitle = (c.Course.CourseDetails.Description ?? "").Split('.', StringSplitOptions.None).FirstOrDefault() ?? string.Empty,
                    CategoryName = c.Course.courseCategory.Name,
                    Total_Price = c.Course.CourseDetails.Total_Price,
                    Actual_Price = c.Course.CourseDetails.Actual_Price,
                    Discount_Percent = c.Course.CourseDetails.Discount_Percent,
                    Thumbnail_Url = c.Course.CourseDetails.Thumbnail_Url
                }).ToList();
        }

        public void RemoveFromCart(int studentId, int courseId)
        {
            var item = _context.Carts.FirstOrDefault(c => c.StudentId == studentId && c.CourseId == courseId);
            if (item != null)
            {
                _context.Carts.Remove(item);
                _context.SaveChanges();
            }
        }

        public int GetCartCount(int studentId)
        {
            return _context.Carts.Count(c => c.StudentId == studentId);
        }
    }
}
