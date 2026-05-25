namespace SkillForge.Models
{
    // Search result view model
    public class SearchResultVM
    {
        public string? Keyword { get; set; }
        public List<CourseCardVM> ExactMatches { get; set; } = new();
        public List<CourseCardVM> RelatedCourses { get; set; } = new();
    }
}
