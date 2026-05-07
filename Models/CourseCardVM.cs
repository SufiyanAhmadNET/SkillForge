

namespace SkillForge.Models
{
    public class CourseCardVM
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? CategoryName { get; set; }
        public string? Difficulty { get; set; }

        public bool? IsWishListed { get; set; } = false;
        public decimal Actual_Price { get; set; }
        public decimal Total_Price { get; set; }
        public int Duration_Weeks { get; set; }
        public float Discount_Percent { get; set; }
        public string? Thumbnail_Url { get; set; }
        public int courseId { get; set; }
        public int ProgressPercentage { get; set; }
    }
}
