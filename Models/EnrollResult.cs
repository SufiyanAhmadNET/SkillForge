namespace SkillForge.Models
{
    public class EnrollResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? RazorpayOrderId { get; set; }
        public int Amount { get; set; }   // paise
        public string? CourseTitle { get; set; }
        public int EnrollmentId { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentMobile { get; set; }
        public string? StudentName { get; set; }
    }
}
