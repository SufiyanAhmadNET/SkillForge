using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Areas.Instructor.Models
{
    public class InstructorProfile
    {
        [Key]
        public int Pid { get; set; }
        public int InstructorId { get; set; }
        public string FirstName { get; set;}
        public string LastName { get; set; }
        public string PhotoPath { get; set; }

        public string Mobile { get; set; }
        public string Location { get; set; }

        public string Bio { get; set; }
        public string Profession { get; set; }

        //Navigation Property
        public Instructor instructor { get; set; }
    }
}
