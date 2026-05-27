using SkillForge.Areas.Instructor.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SkillForge.Interfaces
{
    public interface IAnalyticsService
    {
        // Instructor Analytics
        Task<InstructorDashboardVM> GetInstructorDashboardStatsAsync(int instructorId);
        Task<List<CourseStatsVM>> GetInstructorCoursesOverviewAsync(int instructorId);
        Task<decimal> GetInstructorRevenueAsync(int instructorId);
        Task<int> GetInstructorStudentCountAsync(int instructorId);
        Task<decimal> GetCourseRevenueAsync(int courseId);
        Task<InstructorEarningsVM> GetInstructorEarningsDashboardAsync(int instructorId, int? year, int? month);

        // Admin Analytics (Placeholders for future implementation)
        Task<int> GetTotalStudentsCountAsync();
        Task<int> GetTotalInstructorsCountAsync();
        Task<decimal> GetTotalPlatformRevenueAsync();
    }
}
