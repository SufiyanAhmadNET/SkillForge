
using System;
using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.Instructor.Models
{
    public class InstructorDashboardVM
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name must be at most 50 characters")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name must be at most 50 characters")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number")]
        public string? Mobile { get; set; }

        [StringLength(100, ErrorMessage = "Location must be at most 100 characters")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Bio must be at most 1000 characters")]
        public string? Bio { get; set; }

        [StringLength(100, ErrorMessage = "Profession must be at most 100 characters")]
        public string? Profession { get; set; }

        public string? PhotoPath { get; set; }
    }
}