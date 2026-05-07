using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }
}
