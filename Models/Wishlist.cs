using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SkillForge.Areas.User.Models;

namespace SkillForge.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        
        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
