namespace SkillForge.Models
{
    public class CourseDetailsVM
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Desciption { get; set; }
        public string? VideoUrl { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public float DiscountPercent { get; set; }
        public List<CourseOutcomes> outcomes { get; set; }
        public string languange { get; set; } = "English";
        public string? supportLanguage { get; set; } = "Hinglish";

        // Instructor course details
        public string? Status { get; set; }
        public int Duration { get; set; }
        public string? Difficulty { get; set; }
        public string? CategoryName { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Syllabus
        public List<CourseModules>? modules { get; set; }   
    }
}
