using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.Admin.Models;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Data;
using SkillForge.Interfaces;
using SkillForge.Services.Instructors.Models;
using SkillForge.Services.Courses.Models;

namespace SkillForge.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly SkillForgeDbContext _context;

        public AdminService(SkillForgeDbContext context)
        {
            _context = context;
        }

        // data for admin dashboard
        public AdminDashboardVM GetDashboardData()
        {
            var totalStudents = _context.Students.Count();
            var totalInstructors = _context.instructors.Count();
            var activeCourses = _context.Courses.Count(c => c.Status == CourseStatus.Approved || c.Status == CourseStatus.Published);
            var completedCourses = _context.Courses.Count(c => c.Status == CourseStatus.Completed);

            var recentApplications = GetAllMentorApplications().Take(5).ToList();
            var recentInstructors = GetAllInstructors().OrderByDescending(i => i.JoinedDate).Take(5).ToList();

            return new AdminDashboardVM
            {
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                ActiveCourses = activeCourses,
                CompletedCourses = completedCourses,
                RecentApplications = recentApplications,
                RecentInstructors = recentInstructors
            };
        }

        // Get all courses for admin review
        public List<AdminCourseReviewVM> GetAllCoursesForReview()
        {
            var courses = _context.Courses
                .Include(c => c.courseCategory)
                .Include(c => c.CourseDetails)
                .ToList();

            var instructors = _context.instructors
                .Include(i => i.Profile)
                .ToList();

            var modules = _context.CourseModules
                .Include(m => m.Lessons)
                .ToList();

            return courses.Select(c => {
                var instructor = instructors.FirstOrDefault(i => i.Id == c.instructor_id);
                var courseModules = modules.Where(m => m.CourseId == c.Id).ToList();
                
                return new AdminCourseReviewVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    InstructorName = instructor?.Profile != null ? (instructor.Profile.FirstName + " " + instructor.Profile.LastName).Trim() : "Unknown",
                    Category = c.courseCategory?.Name ?? "Uncategorized",
                    Status = c.Status,
                    SubmittedDate = c.UpdatedAt, // Using UpdatedAt as the latest submission/change date
                    ModuleCount = courseModules.Count,
                    LessonCount = courseModules.Sum(m => m.Lessons.Count),
                    ThumbnailUrl = c.CourseDetails?.Thumbnail_Url,
                    Price = c.CourseDetails?.Total_Price ?? 0,
                    DurationWeeks = c.CourseDetails?.Duration_Weeks ?? 0
                };
            })
            .OrderByDescending(c => c.Status == CourseStatus.PendingReview)
            .ThenByDescending(c => c.SubmittedDate)
            .ToList();
        }

        // Update course status (Approve/Reject)
        public bool UpdateCourseStatus(int courseId, CourseStatus status, string? rejectionReason = null)
        {
            var course = _context.Courses.Find(courseId);
            if (course == null) return false;

            course.Status = status;
            course.Rejection_Reason = rejectionReason;
            course.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }

        // Get all applications for admin review
        public List<MentorApplicationListVM> GetAllMentorApplications()
        {
            return _context.MentorApplications
                .Include(m => m.Instructor)
                    .ThenInclude(i => i.Profile)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MentorApplicationListVM
                {
                    Id = m.Id,
                    InstructorName = (m.Instructor.Profile.FirstName + " " + m.Instructor.Profile.LastName).Trim(),
                    Email = m.Instructor.Email,
                    Expertise = m.Instructor.Profile.Expertise ?? "Not specified",
                    Experience = m.Instructor.Profile.YearsExperience ?? 0,
                    AppliedDate = m.CreatedAt,
                    Status = m.Status,
                    ResumePath = m.ResumePath
                }).ToList();
        }

        // Update application status (Approve/Reject)
        public bool UpdateApplicationStatus(int applicationId, MentorApplicationStatus status, string? adminComment = null)
        {
            var application = _context.MentorApplications.Find(applicationId);
            if (application == null) return false;

            application.Status = status;
            application.AdminComment = adminComment;
            application.ReviewedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }

        // Get all instructors for admin listing
        public List<InstructorListVM> GetAllInstructors()
        {
            var instructors = _context.instructors
                .Include(i => i.Profile)
                .ToList();

            var applications = _context.MentorApplications.ToList();
            var courses = _context.Courses.ToList();
            var enrollments = _context.Enrollments.ToList();

            return instructors.Select(i => {
                var app = applications.Where(a => a.InstructorId == i.Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                var instructorCourses = courses.Where(c => c.instructor_id == i.Id).Select(c => c.Id).ToList();
                var instructorStudents = enrollments.Count(e => instructorCourses.Contains(e.CourseId));

                return new InstructorListVM
                {
                    Id = i.Id,
                    Name = i.Profile != null ? (i.Profile.FirstName + " " + i.Profile.LastName).Trim() : i.Email.Split('@')[0],
                    Email = i.Email,
                    TotalCourses = instructorCourses.Count,
                    TotalStudents = instructorStudents,
                    JoinedDate = i.CreatedAt,
                    Status = app?.Status.ToString() ?? "NotApplied"
                };
            }).ToList();
        }

        // Get all students for admin listing
        public List<StudentListVM> GetAllStudents()
        {
            var students = _context.Students
                .Include(s => s.Profile)
                .ToList();

            var enrollments = _context.Enrollments
                .Include(e => e.Course)
                .ToList();

            return students.Select(s => new StudentListVM
            {
                Id = s.Id,
                Name = s.Profile != null ? (s.Profile.FirstName + " " + s.Profile.LastName).Trim() : s.Email.Split('@')[0],
                Email = s.Email,
                EnrolledCourses = string.Join(", ", enrollments.Where(e => e.StudentId == s.Id).Select(e => e.Course.Title).Take(2)),
                JoinedDate = s.CreatedAt,
                Status = "Active"
            }).ToList();
        }

      
    

    }
}
