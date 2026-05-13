using SkillForge.Models;
using SkillForge.Areas.Instructor.Models;

namespace SkillForge.Interfaces.Instructors
{
    public interface IInstructorService
    {
        InstructorDashboardVM GetInstructorDashboard(int instructorId);
        CourseDetailsVM? GetInstructorCourseDetails(int courseId, int instructorId);
    }
}
