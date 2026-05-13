using SkillForge.Services.Courses.Models;

namespace SkillForge.Models
{
    public class MyCourseVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string? Thumbnail_Url { get; set; }
        public decimal Total_Price { get; set; }
        public CourseStatus Status { get; set; }
    }
}
