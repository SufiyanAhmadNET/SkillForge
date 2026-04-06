using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.Instructor.Models
{
    public class Course
    {
        [Key]
        int Id { get; set; }

         [Required]
        string Title { get; set; }

        [Required]
        string Description { get; set; }
        [Required]
        int Duration { get; set; }
        [Required]

        string Category { get; set; }
        [Required]
        string Difficulty { get; set; }
        [Required]
        decimal Price {  get; set; }
        [Required]
        int Discount_Percentage { get; set; }
        [Required]
        decimal Discount_Total { get; set; }
        [Required]
        decimal Total_Price { get; set; }
        [Required]
        string Outcomes { get; set; }
        string Thumbnail { get; set; }[Required]

        string? Intro_video { get; set; } 


    }
}
