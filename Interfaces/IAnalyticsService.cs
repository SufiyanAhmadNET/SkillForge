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

        // Admin Analytics
        Task<int> GetTotalStudentsCountAsync();
        Task<int> GetTotalInstructorsCountAsync();
        Task<decimal> GetTotalPlatformRevenueAsync();
        Task<decimal> GetPlatformRevenueThisMonthAsync();
        Task<int> GetNewEnrolledCountAsync(int days);
        Task<int> GetNewPublishedCoursesCountAsync(int days);

        // Admin Report Data Retrieval
        Task<AdminEnrollmentReportVM> GetAdminEnrollmentReportAsync(int days);
        Task<AdminSalesReportVM> GetAdminSalesReportAsync(int days);
        Task<AdminStudentReportVM> GetAdminStudentReportAsync(int days);
        Task<AdminInstructorReportVM> GetAdminInstructorReportAsync(int days);
        Task<AdminRevenueReportVM> GetAdminRevenueReportAsync(int days);
        Task<AdminPayoutReportVM> GetAdminPayoutReportAsync(int days);
        Task<AdminApplicationsReportVM> GetAdminApplicationsReportAsync(int days);
    }
}
