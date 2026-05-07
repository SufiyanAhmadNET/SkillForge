using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("UserProgress")]
    public class UserLessonProgress
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int LessonId { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
