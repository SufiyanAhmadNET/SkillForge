using System.ComponentModel.DataAnnotations;
//using SkillForge.Models;
namespace SkillForge.Areas.Instructor.Models
{
    public class Instructor
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        public bool IsVerified { get; set; }
        public string? VerificationToken { get; set; }

        //for Google Login
        public string? GoogleId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //navigation Property
        public InstructorProfile? Profile { get; set; }
    }
}
