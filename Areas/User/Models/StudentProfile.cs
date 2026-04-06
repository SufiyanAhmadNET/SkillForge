using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.User.Models
{
    public class StudentProfile
    {
        [Key]  
        public int Pid {  get; set; }

        public int StudentId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Mobile { get; set; }

        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? Profession { get; set; }

        public string? PhotoPath { get; set; }

        //Navigation Property
        public Student? Student { get; set; }
    }
}
