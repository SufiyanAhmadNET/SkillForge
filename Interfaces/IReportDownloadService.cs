using SkillForge.Models;
using System.Threading.Tasks;

namespace SkillForge.Interfaces
{
    public interface IReportDownloadService
    {
        // Instructor Reports
        Task<byte[]> GenerateCourseFinancialReportPdfAsync(CourseFinancialReportVM data);
        Task<byte[]> GenerateStudentListPdfAsync(CourseStudentListReportVM data);
        Task<byte[]> GenerateMonthlyFinancialReportPdfAsync(MonthlyFinancialReportVM data);
        Task<byte[]> GenerateInstructorGlobalCourseReportPdfAsync(InstructorGlobalCourseReportVM data);

        // Admin Reports
        Task<byte[]> GenerateAdminEnrollmentReportPdfAsync(AdminEnrollmentReportVM data);
        Task<byte[]> GenerateAdminSalesReportPdfAsync(AdminSalesReportVM data);
        Task<byte[]> GenerateAdminStudentReportPdfAsync(AdminStudentReportVM data);
        Task<byte[]> GenerateAdminInstructorReportPdfAsync(AdminInstructorReportVM data);
        Task<byte[]> GenerateAdminRevenueReportPdfAsync(AdminRevenueReportVM data);
        Task<byte[]> GenerateAdminPayoutReportPdfAsync(AdminPayoutReportVM data);
        Task<byte[]> GenerateAdminApplicationsReportPdfAsync(AdminApplicationsReportVM data);
    }
}
