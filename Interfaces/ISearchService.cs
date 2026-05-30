using SkillForge.Models;
using SkillForge.Areas.Admin.Models;

namespace SkillForge.Interfaces
{
    // Search service interface
    public interface ISearchService
    {
        SearchResultVM SearchCourses(string keyword, int studentId = 0);
        List<StudentListVM> SearchStudents(string keyword);
    }
}
