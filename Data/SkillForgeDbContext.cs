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
        public DbSet<CourseModules> CourseModules { get; set; }
        public DbSet<CourseLesson> CourseLessons { get; set; }
        public DbSet<UserLessonProgress> UserProgress { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Cart> Carts { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Course -> Modules -> Lessons relationship
            modelBuilder.Entity<CourseModules>()
                .HasOne(m => m.Course)
                .WithMany()
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseLesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // convert enum to string
            modelBuilder.Entity<Course>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // same for Difficulty in CourseDetails
            modelBuilder.Entity<CourseDetails>()
                .Property(cd => cd.Difficulty)
                .HasConversion<string>();

        modelBuilder.Entity<Course>()
              .HasOne(c => c.CourseDetails)
               .WithOne(cd => cd.Course)
                .HasForeignKey<CourseDetails>(cd => cd.CourseId);

            // enrollment status is stored as string
            modelBuilder.Entity<Enrollment>()
                .Property(e => e.Status)
                .HasConversion<string>();

            // payment status - stored as string
            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // one enrollment - one payment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Payment)
                .WithOne(p => p.Enrollment)
                .HasForeignKey<Payment>(p => p.EnrollmentId);

            // prevent duplicate enrollment — one student, one course
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();
        }
    }
}