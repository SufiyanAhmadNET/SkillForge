using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Interfaces.Instructors;

namespace SkillForge.Services.Instructors
{
    public class InstructorService : IInstructorService
    {
        private readonly SkillForgeDbContext _context;
        public InstructorService(SkillForgeDbContext context)
        {
            _context = context;
        }
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
                .Where(e => courseIds.Contains(e.CourseId))
                .Include(e => e.Student)
                    .ThenInclude(s => s.Profile)
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
                AvgRating = 5.0, 
                ActiveCourses = courses.Select(c => new CourseStatsVM
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    StudentCount = enrollments.Count(e => e.CourseId == c.Id),
                    Rating = 5.0 
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
        public CourseDetailsVM? GetInstructorCourseDetails(int courseId, int instructorId)
        {
            var course = _context.Courses
                .Where(c => c.Id == courseId && c.instructor_id == instructorId)
                .Include(c => c.CourseDetails)
                .Include(c => c.CourseOutcomes)
                .Include(c => c.courseCategory)
                .FirstOrDefault();
            if (course == null) return null;

            var courseLessons = _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .SelectMany(m => m.Lessons)
                .ToList();
            var totalLessons = courseLessons.Count;
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
                        Progress = progress 
                    };
                }).ToList()
            };
        }
    }
}
