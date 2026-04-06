using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.User.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        public required string Email { get; set; } = null!;

        public string? Password { get; set; }

        public bool IsVerified { get; set; } = false;

        public string? VerificationToken { get; set; }

        public DateTime? VerificationTokenExpiry { get; set; }

        //for Google Login
        public string? GoogleId { get; set; }

        //Navigation Property
        public StudentProfile Profile { get; set; }

    }

}
