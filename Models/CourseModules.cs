using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("Syllabus")]
    public class CourseModules
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Course")]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Required]
        public string ModuleName { get ; set; }

        // Navigation property for lessons
        public List<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
        
        // Navigation property for course
        public Course Course { get; set; }
    } 
}
