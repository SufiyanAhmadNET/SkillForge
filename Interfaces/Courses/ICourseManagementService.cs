using SkillForge.Models;
using Microsoft.AspNetCore.Http;
using SkillForge.Services.Courses.Models;

namespace SkillForge.Interfaces.Courses
{
    // Course management service interface
    public interface ICourseManagementService
    {
        // Add new course
        CourseReturn AddCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string action = "draft");
        
        // Update existing course
        CourseReturn UpdateCourse(CourseVM courseVM, int instructorId, IFormFile thumbnailFile, IFormFile videoFile, string youtubeUrl, string videoType, string action = "draft");
        
        // Get instructor courses
        List<MyCourseVM> MyCourses(int instructorId);
        
        // Get course for editing
        CourseVM? GetCourseForEdit(int courseId, int instructorId);
        
        // Delete course
        bool DeleteCourse(int courseId, int instructorId);
    }
}
