using Microsoft.EntityFrameworkCore;
using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Interfaces.Courses;

namespace SkillForge.Services.Courses
{
    public class CourseContentService : ICourseContentService
    {
        private readonly SkillForgeDbContext _context;

        public CourseContentService(SkillForgeDbContext context)
        {
            _context = context;
        }

        public bool AddModule(int courseId, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return false;
            var module = new CourseModules
            {
                CourseId = courseId,
                ModuleName = moduleName
            };
            _context.CourseModules.Add(module);
            return _context.SaveChanges() > 0;
        }

        public bool AddLesson(int moduleId, string title, string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            var lesson = new CourseLesson
            {
                ModuleId = moduleId,
                Title = title,
                VideoUrl = videoUrl,
                Order = _context.CourseLessons.Count(l => l.ModuleId == moduleId) + 1
            };
            _context.CourseLessons.Add(lesson);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteModule(int moduleId)
        {
            var module = _context.CourseModules.Find(moduleId);
            if (module == null) return false;
            _context.CourseModules.Remove(module);
            return _context.SaveChanges() > 0;
        }

        public bool DeleteLesson(int lessonId)
        {
            var lesson = _context.CourseLessons.Find(lessonId);
            if (lesson == null) return false;
            _context.CourseLessons.Remove(lesson);
            return _context.SaveChanges() > 0;
        }
    }
}
