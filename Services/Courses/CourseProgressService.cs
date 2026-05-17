using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Interfaces;

namespace SkillForge.Services.Courses
{
    public class CourseProgressService : ICourseProgressService
    {
        private readonly SkillForgeDbContext _context;
        public CourseProgressService(SkillForgeDbContext context)
        {
            _context = context;
        }
        public bool MarkLessonAsComplete(int studentId, int lessonId)
        {
            var existing = _context.UserProgress
                .FirstOrDefault(p => p.StudentId == studentId && p.LessonId == lessonId);
            if (existing != null)
            {
                existing.IsCompleted = !existing.IsCompleted;
                _context.UserProgress.Update(existing);
            }
            else
            {
                _context.UserProgress.Add(new UserLessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = true
                });
            }
            return _context.SaveChanges() > 0;
        }
        public List<int> GetCompletedLessonIds(int studentId, int courseId)
        {
            var lessonIds = _context.CourseModules
                .Where(m => m.CourseId == courseId)
                .SelectMany(m => m.Lessons)
                .Select(l => l.Id)
                .ToList();
            return _context.UserProgress
                .Where(p => p.StudentId == studentId && p.IsCompleted && lessonIds.Contains(p.LessonId))
                .Select(p => p.LessonId)
                .ToList();
        }
    }
}
