using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.Instructor.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int Duration { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Difficulty { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Discount_Percentage { get; set; }

        [Required]
        public decimal Discount_Total { get; set; }

        [Required]
        public decimal Total_Price { get; set; }

        [Required]
        public string Outcomes { get; set; } = string.Empty;

        [Required]
        public string? Thumbnail { get; set; }

        public string? Intro_video { get; set; }


    }
}
