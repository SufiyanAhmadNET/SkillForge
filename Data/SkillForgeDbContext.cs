using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.User.Models;
using SkillForge.Areas.Instructor.Models;

namespace SkillForge.Data
{
    public class SkillForgeDbContext : DbContext
    {
        public SkillForgeDbContext(DbContextOptions<SkillForgeDbContext> options) : base(options)
        { }

        //==============
        //Student DBcontext
        //==============
        public virtual DbSet<Student> Students { get; set; }

        //STudent Profile 
        public virtual DbSet<StudentProfile> StudentProfiles { get; set; }



        //==============
        //Instructor DBcontext
        //==============
        public DbSet<Instructor> instructors { get; set; }

        //Instructor Profile 
        public DbSet<InstructorProfile> instructorProfiles { get; set; }

    }
}