using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("Lessons")]
    public class CourseLesson
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Module")]
        [Column("module_id")]
        public int ModuleId { get; set; }

        [Required]
        public string Title { get; set; }

        public string? VideoUrl { get; set; } // YouTube Link

        public int Order { get; set; }

        // Navigation property
        public CourseModules Module { get; set; }
    }
}
