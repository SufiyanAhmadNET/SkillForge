using System.Collections.Generic;
using SkillForge.Areas.Instructor.Models;

namespace SkillForge.Models
{
    // Reusable Instructor Info for all reports
    public class InstructorInfoVM
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
    }

    // <Course Name> Financial Report
    public class CourseFinancialReportVM
    {
        public InstructorInfoVM Instructor { get; set; } = new();
        public string CourseTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PublishDate { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal SellingPrice { get; set; }

        public int TotalStudents { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal NetEarnings { get; set; }

        public List<CourseTransactionVM> Transactions { get; set; } = new();
    }

    public class CourseTransactionVM
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string EnrollmentDate { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
    }

    // Enrolled Students for <Course Name> Course
    public class CourseStudentListReportVM
    {
        public InstructorInfoVM Instructor { get; set; } = new();
        public string CourseTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public List<StudentRosterItemVM> Students { get; set; } = new();
    }

    public class StudentRosterItemVM
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string EnrollmentDate { get; set; } = string.Empty;
    }

    // <Month> <Year> Financial Report
    public class MonthlyFinancialReportVM
    {
        public InstructorInfoVM Instructor { get; set; } = new();
        public string ReportMonth { get; set; } = string.Empty;
        
        public int TotalCourses { get; set; }
        public decimal TotalEarning { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal NetEarnings { get; set; }

        public List<MonthlyCourseBreakdownVM> CourseBreakdowns { get; set; } = new();
    }

    public class MonthlyCourseBreakdownVM
    {
        public string CourseName { get; set; } = string.Empty;
        public int NewStudents { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal NetEarnings { get; set; }
    }

    // Global Course Performance Report
    public class InstructorGlobalCourseReportVM
    {
        public InstructorInfoVM Instructor { get; set; } = new();
        public List<CourseEarningsItemVM> CourseEarnings { get; set; } = new();
        public decimal TotalGrossRevenue { get; set; }
        public decimal TotalPlatformFee { get; set; }
        public decimal TotalNetEarnings { get; set; }
    }

    // ==========================================
    // ADMIN REPORTS VIEW MODELS
    // ==========================================

    public class AdminBaseReportVM
    {
        public string Title { get; set; } = string.Empty;
        public string GeneratedDate { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
    }

    // Enrollment Report
    public class AdminEnrollmentReportVM : AdminBaseReportVM
    {
        public int TotalEnrollments { get; set; }
        public int TotalCourses { get; set; }
        public int UniqueStudents { get; set; }
        public List<EnrollmentReportItemVM> Enrollments { get; set; } = new();
    }

    public class EnrollmentReportItemVM
    {
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string EnrollmentDate { get; set; } = string.Empty;
    }

    // Sales Report
    public class AdminSalesReportVM : AdminBaseReportVM
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public List<SalesReportItemVM> Sales { get; set; } = new();
    }

    public class SalesReportItemVM
    {
        public string OrderId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PurchaseDate { get; set; } = string.Empty;
    }

    // Student Report
    public class AdminStudentReportVM : AdminBaseReportVM
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalEnrollments { get; set; }
        public List<StudentReportItemVM> Students { get; set; } = new();
    }

    public class StudentReportItemVM
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JoinedDate { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public string Status { get; set; } = "Active";
    }

    // Instructor Report
    public class AdminInstructorReportVM : AdminBaseReportVM
    {
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudentsTaught { get; set; }
        public List<InstructorReportItemVM> Instructors { get; set; } = new();
    }

    public class InstructorReportItemVM
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Courses { get; set; }
        public int Students { get; set; }
        public string JoinedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // Revenue Report
    public class AdminRevenueReportVM : AdminBaseReportVM
    {
        public decimal GrossRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgRevenuePerOrder { get; set; }
        public List<RevenueReportItemVM> RevenueData { get; set; } = new();
    }

    public class RevenueReportItemVM
    {
        public string Date { get; set; } = string.Empty;
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
    }

    // Instructor Payout Report
    public class AdminPayoutReportVM : AdminBaseReportVM
    {
        public decimal TotalInstructorRevenue { get; set; }
        public decimal TotalPayouts { get; set; }
        public List<PayoutReportItemVM> Payouts { get; set; } = new();
    }

    public class PayoutReportItemVM
    {
        public string InstructorName { get; set; } = string.Empty;
        public int Courses { get; set; }
        public decimal RevenueGenerated { get; set; }
        public decimal Commission { get; set; }
        public decimal PayoutAmount { get; set; }
    }

    // Instructor Applications Report
    public class AdminApplicationsReportVM : AdminBaseReportVM
    {
        public int TotalApplications { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public List<ApplicationReportItemVM> Applications { get; set; } = new();
    }

    public class ApplicationReportItemVM
    {
        public string ApplicantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string AppliedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
