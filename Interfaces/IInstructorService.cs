using SkillForge.Models;
using SkillForge.Areas.Instructor.Models;

namespace SkillForge.Interfaces
{
    public interface IInstructorService
    {
        Task<CourseDetailsVM?> GetInstructorCourseDetails(int courseId, int instructorId);
    }
}
