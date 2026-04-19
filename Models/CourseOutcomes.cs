
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    public class CourseOutcomes
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public string Outcome { get; set; } 

        //Navigation Prop
        public Course course { get; set; }

        public static implicit operator string(CourseOutcomes v)
        {
            throw new NotImplementedException();
        }
    }
}
