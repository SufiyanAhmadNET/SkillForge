using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.User.Models;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Models;

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

        //course
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseOutcomes> CourseOutcomes { get; set; }
        public DbSet<CourseDetails> CourseDetails { get; set; }
       public DbSet<Course_Category> course_Categories { get; set; }



        //enum to string
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // convert enum to string
            modelBuilder.Entity<Course>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // same for Difficulty in CourseDetails
            modelBuilder.Entity<CourseDetails>()
                .Property(cd => cd.Difficulty)
                .HasConversion<string>();
        }
    }
}