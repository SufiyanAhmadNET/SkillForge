using SkillForge.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillForge.Areas.Instructor.Models
{
    public class InstructorDashboardVM
    {
        // profile info
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhotoPath { get; set; }
        public string? Mobile { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }
        public string? Profession { get; set; }
        
        // professional and social presence
        public string? Headline { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? Skills { get; set; }

        // stats
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalEarnings { get; set; }
        public double AvgRating { get; set; } = 4.8; // default until rating system implemented

        // lists for dashboard tables
        public List<CourseStatsVM> ActiveCourses { get; set; } = new();
        public List<RecentEnrollmentVM> RecentEnrollments { get; set; } = new();
    }

    public class CourseStatsVM
    {
        public int CourseId { get; set; }
        public string? Title { get; set; }
        public int StudentCount { get; set; }
        public string? Status { get; set; }
        public double Rating { get; set; } = 4.9; 
    }

    public class RecentEnrollmentVM
    {
        public string? StudentName { get; set; }
        public string? CourseTitle { get; set; }
        public string? EnrolledDate { get; set; }
        public string? Initial { get; set; }
    }
}
