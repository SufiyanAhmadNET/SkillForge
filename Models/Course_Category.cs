using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("course_category")]
    public class Course_Category
    {
        [Key]
      public  int Id { get; set; }
       public string Name { get; set; }

        //One Category map to many courses
        public List<Course> Courses { get; set; } = new List<Course>(); // avoid null

    }
}
