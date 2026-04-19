using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("Syllabus")]
    public class CourseModules
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("course_id")]
        public int CourseId { get; set; }

        [Required]
        public string ModuleName { get ; set; }

    } 
}
