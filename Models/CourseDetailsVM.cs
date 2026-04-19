namespace SkillForge.Models
{
    public class CourseDetailsVM
    {
        public string Ttile { get; set; }
        public string? SubTitle { get; set; }
        public string? Desciption { get; set; }
        public string? VideoUrl { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public float DiscountPercent { get; set; }
        public List<CourseOutcomes> outcomes { get; set; }
        public string languange { get; set; } = "English";

        public string? supportLanguage { get; set; } = "Hinglish";

        //Syllabus
        public List<CourseModules>? modules { get; set; }   

    }
}
