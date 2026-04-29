using Razorpay.Api;
using SkillForge.Areas.User.Models;
using SkillForge.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // who enrolled
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        // which course
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        // enrollment status — pending until payment verified
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        // navg props
        public Student Student { get; set; }
        public Course Course { get; set; }

        // one enrollment = one payment record
        public Payment? Payment { get; set; }
    }

    public enum EnrollmentStatus
    {
        Pending,    // order created, payment not done
        Active,     // payment verified, student can access
        Failed,     // payment failed
        Refunded    // money back
    }
}