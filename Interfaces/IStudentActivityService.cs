using SkillForge.Models;
using SkillForge.Areas.User.Models;

namespace SkillForge.Interfaces
{
    public interface IStudentActivityService
    {
        List<CourseCardVM> GetEnrolledCourses(int studentId);
        bool ToggleWishlist(int studentId, int courseId);
        List<CourseCardVM> GetWishlist(int studentId);
        DashboardVM GetStudentDashboard(int studentId);
        OrderHistoryVM GetStudentOrders(int studentId);
        bool AddToCart(int studentId, int courseId);
        List<CourseCardVM> GetCartItems(int studentId);
        void RemoveFromCart(int studentId, int courseId);
        int GetCartCount(int studentId);
        bool IsProfileComplete(int studentId);
    }
}
