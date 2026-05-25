using SkillForge.Models;

namespace SkillForge.Interfaces
{
    // Search service interface
    public interface ISearchService
    {
        SearchResultVM SearchCourses(string keyword, int studentId = 0);
    }
}
