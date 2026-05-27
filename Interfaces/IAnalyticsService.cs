using SkillForge.Areas.Instructor.Models;
using SkillForge.Models;
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
        
        // Report Data Retrieval
        Task<CourseFinancialReportVM?> GetCourseFinancialReportAsync(int courseId, int instructorId);
        Task<CourseStudentListReportVM?> GetCourseStudentListReportAsync(int courseId, int instructorId);
        Task<MonthlyFinancialReportVM> GetMonthlyFinancialReportAsync(int instructorId, int year, int month);
        Task<InstructorGlobalCourseReportVM> GetInstructorGlobalCourseReportAsync(int instructorId);

        // Admin Analytics (Placeholders for future implementation)
        Task<int> GetTotalStudentsCountAsync();
        Task<int> GetTotalInstructorsCountAsync();
        Task<int> GetTotalPlatformRevenueAsync();
    }
}
