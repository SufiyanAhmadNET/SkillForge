namespace SkillForge.Interfaces.Courses
{
    public interface ICourseProgressService
    {
        bool MarkLessonAsComplete(int studentId, int lessonId);
        List<int> GetCompletedLessonIds(int studentId, int courseId);
    }
}
