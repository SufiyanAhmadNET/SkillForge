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
}
