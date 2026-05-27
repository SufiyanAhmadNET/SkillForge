using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Data;
using SkillForge.Interfaces;
using SkillForge.Models;
using SkillForge.Services.Instructors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillForge.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly SkillForgeDbContext _context;

        public AnalyticsService(SkillForgeDbContext context)
        {
            _context = context;
        }

        public async Task<InstructorDashboardVM> GetInstructorDashboardStatsAsync(int instructorId)
        {
            var instructor = await _context.instructors
                .AsNoTracking()
                .Include(i => i.Profile)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            var application = await _context.MentorApplications
                .AsNoTracking()
                .Where(m => m.InstructorId == instructorId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            var courseIds = await _context.Courses
                .AsNoTracking()
                .Where(c => c.instructor_id == instructorId)
                .Select(c => c.Id)
                .ToListAsync();

            var totalCourses = courseIds.Count;
            var totalStudents = await GetInstructorStudentCountAsync(instructorId);
            var totalEarnings = await GetInstructorRevenueAsync(instructorId);

            var activeCoursesOverview = await GetInstructorCoursesOverviewAsync(instructorId);

            return new InstructorDashboardVM
            {
                Email = instructor?.Email,
                FirstName = instructor?.Profile?.FirstName ?? "Instructor",
                LastName = instructor?.Profile?.LastName,
                PhotoPath = instructor?.Profile?.PhotoPath ?? "/images/DefaultProfilePhoto.jfif",
                
                ApplicationStatus = application?.Status ?? MentorApplicationStatus.NotApplied,
                ApplicationComment = application?.AdminComment,

                TotalCourses = totalCourses,
                TotalStudents = totalStudents,
                TotalEarnings = totalEarnings,
                AvgRating = 4.8, // Default placeholder as per requirement

                ActiveCourses = activeCoursesOverview
            };
        }

        public async Task<List<CourseStatsVM>> GetInstructorCoursesOverviewAsync(int instructorId)
        {
            return await _context.Courses
                .AsNoTracking()
                .Where(c => c.instructor_id == instructorId)
                .Select(c => new CourseStatsVM
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id && e.Status == EnrollmentStatus.Active),
                    Rating = 4.9, // Placeholder as requested
                    Earnings = _context.Payments
                        .Where(p => p.Enrollment.CourseId == c.Id && p.Status == PaymentStatus.Success)
                        .Sum(p => (decimal?)p.Amount) ?? 0
                })
                .ToListAsync();
        }

        public async Task<decimal> GetInstructorRevenueAsync(int instructorId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => _context.Courses.Any(c => c.Id == p.Enrollment.CourseId && c.instructor_id == instructorId) 
                            && p.Status == PaymentStatus.Success)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }

        public async Task<int> GetInstructorStudentCountAsync(int instructorId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => _context.Courses.Any(c => c.Id == e.CourseId && c.instructor_id == instructorId) 
                                 && e.Status == EnrollmentStatus.Active);
        }

        // Admin Analytics Implementation
        public async Task<int> GetTotalStudentsCountAsync()
        {
            return await _context.Students.CountAsync();
        }

        public async Task<int> GetTotalInstructorsCountAsync()
        {
            return await _context.instructors.CountAsync();
        }

        public async Task<decimal> GetTotalPlatformRevenueAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.Status == PaymentStatus.Success)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }
    }
}
