using SkillForge.Areas.Admin.Models;
using SkillForge.Services.Courses.Models;
using SkillForge.Services.Instructors.Models;

namespace SkillForge.Interfaces
{
    public interface IAdminService
    {
        AdminDashboardVM GetDashboardData();
        List<MentorApplicationListVM> GetAllMentorApplications();
        bool UpdateApplicationStatus(int applicationId, MentorApplicationStatus status, string? adminComment = null);
        List<InstructorListVM> GetAllInstructors();
        List<StudentListVM> GetAllStudents();

        // Course Review Methods
        List<AdminCourseReviewVM> GetAllCoursesForReview();
        bool UpdateCourseStatus(int courseId, CourseStatus status, string? rejectionReason = null);
    }
}
