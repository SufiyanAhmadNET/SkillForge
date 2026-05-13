using SkillForge.Models;

namespace SkillForge.Interfaces.Courses
{
    public interface ICourseQueryService
    {
        CoursePageVM GetPublishedCoursePage(int studentId = 0);
        CourseDetailsVM? GetCourseDetails(int courseId, int studentId = 0);
    }
}
