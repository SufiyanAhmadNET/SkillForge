namespace SkillForge.Interfaces
{
    public interface ICourseContentService
    {
        bool AddModule(int courseId, string moduleName);
        bool AddLesson(int moduleId, string title, string videoUrl);
        bool DeleteModule(int moduleId);
        bool DeleteLesson(int lessonId);
    }
}
