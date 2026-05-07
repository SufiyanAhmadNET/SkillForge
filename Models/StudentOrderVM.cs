using System;

namespace SkillForge.Models
{
    public class StudentOrderVM
    {
        public int OrderId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Online";
    }

    public class OrderHistoryVM
    {
        public List<StudentOrderVM> Orders { get; set; } = new List<StudentOrderVM>();
        public int TotalCourses { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalSaved { get; set; }
    }
}
