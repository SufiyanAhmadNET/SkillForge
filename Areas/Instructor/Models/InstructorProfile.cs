using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Areas.Instructor.Models
{
    public class InstructorProfile
    {
        [Key]
        public int Pid { get; set; }
        public int InstructorId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhotoPath { get; set; }

        public string? Mobile { get; set; }
        public string? Location { get; set; }

        public string? Bio { get; set; }
        public string? Profession { get; set; }

        // professional and social presence
        public string? Headline { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? Skills { get; set; }

        //Navigation Property
        public Instructor? Instructor { get; set; }
    }
}
