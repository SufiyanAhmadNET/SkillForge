namespace SkillForge.Areas.Admin.Models
{
    public class AdminDashboardVM
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int ActiveCourses { get; set; }
        public int CompletedCourses { get; set; }
        public List<MentorApplicationListVM> RecentApplications { get; set; } = new();
        public List<InstructorListVM> RecentInstructors { get; set; } = new();
    }
}
