using SkillForge.Services;
using System.ComponentModel.DataAnnotations;

namespace SkillForge.Models
{
    public class CourseVM   
    {
        
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Required(ErrorMessage = "Title is required")] public string Title { get; set; }
        public int Id { get; set; }
        public string? Category_Id { get; set;}

        //enum class type for course status
        public CourseStatus CourseStatus { get; set; } = CourseStatus.Approved;

        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? Rejection_Reason { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        public IEnumerable<string>? outcome { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Actual price must be 0 or more")]
        public decimal Actual_Price { get; set; }

       
        public int Discount_Percent { get; set; }
       
        public decimal Total_Price { get; set; }

        [Required(ErrorMessage = "Select a difficulty level")]
        public Course_Difficulty? Difficulty { get; set; }

        [Required(ErrorMessage = "Add Duration")]
        public int? Duration_Weeks { get; set; }

        public string? Thumbnail_Url { get; set; }

        public string? Intro_Video_Url { get; set; }

        // Syllabus for Model Binding
        public List<ModuleVM> Syllabus { get; set; } = new List<ModuleVM>();
    }

    public class ModuleVM
    {
        public string ModuleName { get; set; }
        public List<LessonVM> Lessons { get; set; } = new List<LessonVM>();
    }

    public class LessonVM
    {
        public string Title { get; set; }
        public string? VideoUrl { get; set; }
    }
}
