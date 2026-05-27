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

        public async Task<decimal> GetCourseRevenueAsync(int courseId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.Enrollment.CourseId == courseId && p.Status == PaymentStatus.Success)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }

        public async Task<InstructorEarningsVM> GetInstructorEarningsDashboardAsync(int instructorId, int? year, int? month)
        {
            var now = DateTime.UtcNow;
            int selYear = year ?? now.Year;
            int selMonth = month ?? now.Month;

            // Fetch all successful payments for this instructor's courses
            var allPayments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Enrollment.Course)
                .Where(p => p.Enrollment.Course.instructor_id == instructorId && p.Status == PaymentStatus.Success)
                .ToListAsync();

            // Fetch all active enrollments for this instructor's courses
            var allEnrollments = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Course.instructor_id == instructorId && e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            // Summary Totals
            var totalEarned = allPayments.Sum(p => p.Amount);
            var thisMonthEarnings = allPayments.Where(p => p.CreatedAt.Year == now.Year && p.CreatedAt.Month == now.Month).Sum(p => p.Amount);
            var prevMonthEarnings = allPayments.Where(p => p.CreatedAt.Year == (now.Month == 1 ? now.Year - 1 : now.Year) && p.CreatedAt.Month == (now.Month == 1 ? 12 : now.Month - 1)).Sum(p => p.Amount);
            
            double growth = 0;
            if (prevMonthEarnings > 0)
                growth = (double)((thisMonthEarnings - prevMonthEarnings) / prevMonthEarnings * 100);

            var payingStudents = allPayments.Select(p => p.Enrollment.StudentId).Distinct().Count();
            var newStudentsWeek = allEnrollments.Count(e => e.EnrolledAt >= now.AddDays(-7));

            // Course Wise Breakdown
            var courseEarnings = allPayments
                .GroupBy(p => new { p.Enrollment.CourseId, p.Enrollment.Course.Title })
                .Select(g => new CourseEarningsItemVM
                {
                    CourseId = g.Key.CourseId,
                    CourseTitle = g.Key.Title,
                    EnrolledStudents = g.Select(p => p.Enrollment.StudentId).Distinct().Count(),
                    GrossRevenue = g.Sum(p => p.Amount),
                    PlatformFee = g.Sum(p => p.Amount) * 0.20m, // 20% fee
                    InstructorEarnings = g.Sum(p => p.Amount) * 0.80m,
                    PricePerStudent = g.Any() ? g.Average(p => p.Amount) : 0
                }).ToList();

            // Monthly Breakdown (Historical)
            var monthlyBreakdown = allPayments
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Select(g => new MonthlyBreakdownItemVM
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    NewStudents = g.Select(p => p.Enrollment.StudentId).Distinct().Count(),
                    GrossRevenue = g.Sum(p => p.Amount),
                    InstructorEarnings = g.Sum(p => p.Amount) * 0.80m,
                    PayoutStatus = (g.Key.Year < now.Year || (g.Key.Year == now.Year && g.Key.Month < now.Month)) ? "Paid" : "Pending"
                }).ToList();

            // Filter available years
            var availableYears = allPayments.Select(p => p.CreatedAt.Year).Distinct().OrderByDescending(y => y).ToList();
            if (!availableYears.Any()) availableYears.Add(now.Year);

            return new InstructorEarningsVM
            {
                TotalEarned = totalEarned * 0.80m, // Instructor's 80% share
                ThisMonthEarnings = thisMonthEarnings * 0.80m,
                PreviousMonthEarnings = prevMonthEarnings * 0.80m,
                GrowthPercentage = Math.Round(growth, 1),
                PendingPayout = thisMonthEarnings * 0.80m, // Conceptually pending for current month
                PayingStudents = payingStudents,
                NewStudentsThisWeek = newStudentsWeek,
                CourseEarnings = courseEarnings,
                MonthlyBreakdown = monthlyBreakdown,
                AvailableYears = availableYears,
                SelectedYear = selYear,
                SelectedMonth = selMonth
            };
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
