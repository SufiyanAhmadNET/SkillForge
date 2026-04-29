using SkillForge.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillForge.Models
{
    public class CourseDetails
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Description { get; set; }
        public decimal Actual_Price { get; set; }
        public int Discount_Percent { get; set; }
        public decimal Total_Price { get; set; }

        [Column("diff_level")]
        public Course_Difficulty Difficulty { get; set; }
        public int Duration_Weeks { get; set; }
        public string? Thumbnail_Url { get; set; }
        public string? Intro_Video_Url { get; set; }

        //Navigation Prop
        public Course Course { get; set; }

    }
}
