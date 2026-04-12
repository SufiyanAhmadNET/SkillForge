using System.ComponentModel.DataAnnotations;

namespace SkillForge.Models
{
    public class Course_Category
    {
        [Key]
      public  int Id { get; set; }
       public string Name { get; set; }

        //One Category map to many courses
        public List<Course> Courses { get; set; } = new List<Course>(); // avoid null

    }
}
