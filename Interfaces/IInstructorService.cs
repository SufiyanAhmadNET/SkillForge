using SkillForge.Models;
using SkillForge.Areas.Instructor.Models;

namespace SkillForge.Interfaces
{
    public interface IInstructorService
    {
        InstructorDashboardVM GetInstructorDashboard(int instructorId);
        CourseDetailsVM? GetInstructorCourseDetails(int courseId, int instructorId);
    }
}
