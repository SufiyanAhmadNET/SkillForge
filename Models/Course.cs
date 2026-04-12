using SkillForge.Areas.Instructor.Models;
using SkillForge.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("instructor")]
        public int instructor_id { get; set; }

        [ForeignKey("courseCategory")]
        public int category_id { get; set; }

        [Required]
        public string Title { get; set; }

        //enum class type for course status
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public string? Rejection_Reason { get; set; }

        //Navigation Props
        public Instructor instructor { get; set; }
        public Course_Category courseCategory { get; set; }

        //Navigation Prop for multiple outcomes to maap with one course
        public List<CourseOutcomes> CourseOutcomes { get; set; }

        //one to one
        [ForeignKey("course_id")]
        public CourseDetails CourseDetails { get; set; }

    }
}