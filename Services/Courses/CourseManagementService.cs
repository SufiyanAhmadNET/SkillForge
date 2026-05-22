using Microsoft.EntityFrameworkCore;
using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Interfaces;
using SkillForge.Services.Courses.Models;

namespace SkillForge.Services.Courses
{
    // Course management service implementation
    public class CourseManagementService : ICourseManagementService
    {
        private readonly SkillForgeDbContext _context;
        private readonly IMediaService _mediaService;
        public CourseManagementService(SkillForgeDbContext context, IMediaService mediaService)
        {
            _context = context;
            _mediaService = mediaService;
        }

        // Add new course with details and syllabus
        public CourseReturn AddCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string submitAction = "draft")
        {
            try
            {
                EnsureDefaultCategories();
                
                // Input validation
                if (courseVM == null)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Invalid course data." };
                if (string.IsNullOrWhiteSpace(courseVM.Title))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Course title is required." };
                if (string.IsNullOrWhiteSpace(courseVM.Description))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Course description is required." };
                if (string.IsNullOrWhiteSpace(courseVM.ShortSummary))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Short summary is required." };
                if (!courseVM.Duration_Weeks.HasValue || courseVM.Duration_Weeks.Value <= 0)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Duration must be greater than 0 weeks." };
                if (courseVM.Actual_Price < 0)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Price cannot be negative." };
                if (courseVM.Discount_Percent < 0 || courseVM.Discount_Percent > 100)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Discount must be between 0 and 100." };
                if (!courseVM.Difficulty.HasValue || courseVM.Difficulty.Value == Course_Difficulty.None)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Select a valid difficulty level." };
                
                // Clean outcomes
                var outcomes = courseVM.outcome?
                    .Select(o => o?.Trim())
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o!)
                    .ToList() ?? new List<string>();
                
                if (!outcomes.Any())
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Add at least one learning outcome." };
                
                // Category check
                if (!int.TryParse(courseVM.Category_Id, out var categoryId) || categoryId <= 0)
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Please select a category." };
                if (!_context.course_Categories.Any(c => c.Id == categoryId))
                    return new CourseReturn { Success = false, message = CourseMessage.EmptyFields, TechnicalMessage = "Selected category is not available." };
                
                bool isSubmitAction = submitAction?.ToLower() == "submit";
                
                // Create course entity
                var course = new Course
                {
                    Title = courseVM.Title,
                    instructor_id = instructorId,
                    category_id = categoryId,
                    Status = isSubmitAction ? CourseStatus.PendingReview : CourseStatus.Draft,
                    Rejection_Reason = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Add course details
                course.CourseDetails = new CourseDetails
                {
                    Description = courseVM.Description,
                    ShortSummary = courseVM.ShortSummary,
                    Actual_Price = courseVM.Actual_Price,
                    Discount_Percent = courseVM.Discount_Percent,
                    Total_Price = courseVM.Actual_Price - (courseVM.Actual_Price * courseVM.Discount_Percent / 100),
                    Duration_Weeks = courseVM.Duration_Weeks ?? 0,
                    Difficulty = courseVM.Difficulty.Value,
                    Thumbnail_Url = _mediaService.SaveThumbnail(thumbnailFile),
                    Intro_Video_Url = _mediaService.HandleVideo(videoFile, youtubeUrl, videoType),
                };
                
                // Map outcomes
                course.CourseOutcomes = outcomes
                    .Select(o => new CourseOutcomes { Outcome = o }).ToList();
                
                _context.Courses.Add(course);
                _context.SaveChanges();
                
                // Save syllabus modules and lessons
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

        // Update existing course details and syllabus
        public CourseReturn UpdateCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string submitAction = "draft")
        {
            try
            {
                // Fetch course for update
                var course = _context.Courses
                    .Where(c => c.Id == courseVM.Id && c.instructor_id == instructorId)
                    .Include(c => c.CourseDetails)
                    .Include(c => c.CourseOutcomes)
                    .FirstOrDefault();
                
                if (course == null)
                    return new CourseReturn { Success = false, message = CourseMessage.CourseNotAdded, TechnicalMessage = "Course not found." };
                
                // Update basic info
                course.Title = courseVM.Title;
                course.category_id = int.Parse(courseVM.Category_Id ?? "0");
                course.UpdatedAt = DateTime.UtcNow;
                
                if (submitAction?.ToLower() == "submit")
                    course.Status = CourseStatus.PendingReview;
                else if (course.Status == CourseStatus.Approved || course.Status == CourseStatus.Published)
                    course.Status = CourseStatus.Draft;
                
                if (course.CourseDetails == null) course.CourseDetails = new CourseDetails();
                
                // Update pricing and specs
                course.CourseDetails.Description = courseVM.Description;
                course.CourseDetails.ShortSummary = courseVM.ShortSummary;
                course.CourseDetails.Actual_Price = courseVM.Actual_Price;
                course.CourseDetails.Discount_Percent = courseVM.Discount_Percent;
                course.CourseDetails.Total_Price = courseVM.Actual_Price - (courseVM.Actual_Price * courseVM.Discount_Percent / 100);
                course.CourseDetails.Duration_Weeks = courseVM.Duration_Weeks ?? 0;
                course.CourseDetails.Difficulty = courseVM.Difficulty ?? Course_Difficulty.Beginner;
                
                // Handle media updates
                if (thumbnailFile != null)
                    course.CourseDetails.Thumbnail_Url = _mediaService.SaveThumbnail(thumbnailFile);
                if (videoFile != null || !string.IsNullOrWhiteSpace(youtubeUrl))
                    course.CourseDetails.Intro_Video_Url = _mediaService.HandleVideo(videoFile, youtubeUrl, videoType);
                
                // Update outcomes
                if (courseVM.outcome != null)
                {
                    _context.CourseOutcomes.RemoveRange(course.CourseOutcomes);
                    course.CourseOutcomes = courseVM.outcome
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => new CourseOutcomes { Outcome = o.Trim() }).ToList();
                }
                
                // Update syllabus (replace all modules)
                if (courseVM.Syllabus != null)
                {
                    var existingModules = _context.CourseModules.Where(m => m.CourseId == course.Id).ToList();
                    _context.CourseModules.RemoveRange(existingModules);
                    
                    foreach (var modVM in courseVM.Syllabus)
                    {
                        if (string.IsNullOrWhiteSpace(modVM.ModuleName)) continue;
                        var module = new CourseModules { CourseId = course.Id, ModuleName = modVM.ModuleName };
                        _context.CourseModules.Add(module);
                        _context.SaveChanges();
                        
                        if (modVM.Lessons != null)
                        {
                            int order = 1;
                            foreach (var les in modVM.Lessons)
                            {
                                if (string.IsNullOrWhiteSpace(les.Title)) continue;
                                _context.CourseLessons.Add(new CourseLesson { ModuleId = module.Id, Title = les.Title, VideoUrl = les.VideoUrl, Order = order++ });
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

        // Get courses created by instructor
        public List<MyCourseVM> MyCourses(int instructorId)
        {
            return _context.Courses
                .Where(c => c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .Select(c => new MyCourseVM
                {
                    CourseId = c.Id,
                    Thumbnail_Url = c.CourseDetails != null ? c.CourseDetails.Thumbnail_Url : string.Empty,
                    Status = c.Status,
                    CategoryName = c.courseCategory != null ? c.courseCategory.Name : "Uncategorized",
                    Title = c.Title,
                    Total_Price = c.CourseDetails != null ? c.CourseDetails.Total_Price : 0
                }).ToList();
        }

        // Fetch course data for editing
        public CourseVM? GetCourseForEdit(int courseId, int instructorId)
        {
            var course = _context.Courses
                .Where(c => c.Id == courseId && c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .Include(c => c.CourseOutcomes)
                .FirstOrDefault();
            
            if (course == null) return null;
            
            // Fetch syllabus
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

            // Map to VM
            return new CourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.CourseDetails?.Description ?? string.Empty,
                ShortSummary = course.CourseDetails?.ShortSummary ?? string.Empty,
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

        // Delete course and related data
        public bool DeleteCourse(int courseId, int instructorId)
        {
            try
            {
                var course = _context.Courses
                    .Where(c => c.Id == courseId && c.instructor_id == instructorId)
                    .Include(c => c.CourseDetails)
                    .Include(c => c.CourseOutcomes)
                    .FirstOrDefault();
                
                if (course == null) return false;
                
                // Remove related details and outcomes
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
        }

        // Initialize default course categories
        private void EnsureDefaultCategories()
        {
            if (_context.course_Categories.Any())
                return;
            
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
    }
}
