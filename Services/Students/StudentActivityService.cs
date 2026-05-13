using Microsoft.EntityFrameworkCore;
using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Areas.User.Models;
using SkillForge.Interfaces.Students;
using SkillForge.Services.Courses.Models;

namespace SkillForge.Services.Students
{
    public class StudentActivityService : IStudentActivityService
    {
        private readonly SkillForgeDbContext _context;

        public StudentActivityService(SkillForgeDbContext context)
        {
            _context = context;
        }

        public List<CourseCardVM> GetEnrolledCourses(int studentId)
        {
            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(e => e.Course)
                    .ThenInclude(c => c.courseCategory)
                .ToList();

            var courseIds = enrollments.Select(e => e.Course.Id).ToList();
            var modules = _context.CourseModules
                .Where(m => courseIds.Contains(m.CourseId))
                .Include(m => m.Lessons)
                .ToList();

            var progress = _context.UserProgress
                .Where(p => p.StudentId == studentId && p.IsCompleted)
                .ToList();

            return enrollments
                .Select(e => {
                    var courseLessons = modules.Where(m => m.CourseId == e.Course.Id).SelectMany(m => m.Lessons).ToList();
                    var totalLessons = courseLessons.Count;
                    var completedLessons = progress.Count(p => courseLessons.Any(cl => cl.Id == p.LessonId));
                    var progressPercentage = totalLessons > 0 ? (int)((float)completedLessons / totalLessons * 100) : 0;
                    return new CourseCardVM
                    {
                        courseId = e.Course.Id,
                        Title = e.Course.Title,
                        SubTitle = e.Course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                        CategoryName = e.Course.courseCategory?.Name ?? "Uncategorized",
                        Difficulty = e.Course.CourseDetails?.Difficulty.ToString() ?? "None",
                        Total_Price = e.Course.CourseDetails?.Total_Price ?? 0,
                        Actual_Price = e.Course.CourseDetails?.Actual_Price ?? 0,
                        Discount_Percent = e.Course.CourseDetails?.Discount_Percent ?? 0,
                        Thumbnail_Url = e.Course.CourseDetails?.Thumbnail_Url,
                        ProgressPercentage = progressPercentage
                    };
                }).ToList();
        }

        public bool ToggleWishlist(int studentId, int courseId)
        {
            var existing = _context.Wishlists
                .FirstOrDefault(w => w.StudentId == studentId && w.CourseId == courseId);
            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                _context.SaveChanges();
                return false;
            }
            else
            {
                _context.Wishlists.Add(new Wishlist
                {
                    StudentId = studentId,
                    CourseId = courseId
                });
                _context.SaveChanges();
                return true;
            }
        }

        public List<CourseCardVM> GetWishlist(int studentId)
        {
            var wishlist = _context.Wishlists
                .Where(w => w.StudentId == studentId)
                .Include(w => w.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(w => w.Course)
                    .ThenInclude(c => c.courseCategory)
                .ToList();

            return wishlist.Select(w => new CourseCardVM
            {
                courseId = w.Course.Id,
                Title = w.Course.Title,
                SubTitle = w.Course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? string.Empty,
                CategoryName = w.Course.courseCategory?.Name ?? "Uncategorized",
                Difficulty = w.Course.CourseDetails?.Difficulty.ToString() ?? "None",
                Total_Price = w.Course.CourseDetails?.Total_Price ?? 0,
                Actual_Price = w.Course.CourseDetails?.Actual_Price ?? 0,
                Discount_Percent = w.Course.CourseDetails?.Discount_Percent ?? 0,
                Thumbnail_Url = w.Course.CourseDetails?.Thumbnail_Url,
                IsWishListed = true
            }).ToList();
        }

        public DashboardVM GetStudentDashboard(int studentId)
        {
            var student = _context.Students
                .Include(s => s.Profile)
                .FirstOrDefault(s => s.Id == studentId);

            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseDetails)
                .ToList();

            var wishlistCount = _context.Wishlists.Count(w => w.StudentId == studentId);

            var recommended = _context.Courses
                .Where(c => c.Status == CourseStatus.Published)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .OrderBy(r => Guid.NewGuid()) 
                .Take(3)
                .Select(c => new CourseCardVM
                {
                    courseId = c.Id,
                    Title = c.Title,
                    CategoryName = c.courseCategory != null ? c.courseCategory.Name : "Uncategorized",
                    Total_Price = c.CourseDetails != null ? c.CourseDetails.Total_Price : 0,
                    Thumbnail_Url = c.CourseDetails != null ? c.CourseDetails.Thumbnail_Url : string.Empty
                }).ToList();

            return new DashboardVM
            {
                Id = student?.Id ?? 0,
                Email = student?.Email,
                FirstName = student?.Profile?.FirstName ?? "Student",
                LastName = student?.Profile?.LastName,
                Mobile = student?.Profile?.Mobile,
                Bio = student?.Profile?.Bio,
                City = student?.Profile?.City,
                Profession = student?.Profile?.Profession,
                PhotoPath = student?.Profile?.PhotoPath ?? "/images/DefaultProfilePhoto.jfif",
                EnrolledCount = enrollments.Count,
                WishlistCount = wishlistCount,
                CertificateCount = 0,
                CompletedCount = 0,
                EnrolledCourses = enrollments.Select(e => new CourseCardVM
                {
                    courseId = e.Course.Id,
                    Title = e.Course.Title,
                    Thumbnail_Url = e.Course.CourseDetails?.Thumbnail_Url,
                    SubTitle = e.Course.CourseDetails?.Description?.Split('.').FirstOrDefault() ?? ""
                }).Take(4).ToList(),
                RecommendedCourses = recommended
            };
        }

        public OrderHistoryVM GetStudentOrders(int studentId)
        {
            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseDetails)
                .Include(e => e.Payment)
                .OrderByDescending(e => e.EnrolledAt)
                .ToList();

            var orders = enrollments.Select(e => new StudentOrderVM
            {
                OrderId = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                ThumbnailUrl = e.Course.CourseDetails?.Thumbnail_Url ?? string.Empty,
                Amount = e.Payment?.Amount ?? 0,
                OrderDate = e.EnrolledAt,
                PaymentStatus = e.Payment?.Status.ToString() ?? "Pending",
                RazorpayOrderId = e.Payment?.RazorpayOrderId ?? "N/A"
            }).ToList();

            return new OrderHistoryVM
            {
                Orders = orders,
                TotalCourses = orders.Count(o => o.PaymentStatus == "Success"),
                TotalSpent = orders.Where(o => o.PaymentStatus == "Success").Sum(o => o.Amount),
                TotalSaved = enrollments
                    .Where(e => e.Payment?.Status == PaymentStatus.Success)
                    .Sum(e => (e.Course.CourseDetails?.Actual_Price ?? 0) - (e.Payment?.Amount ?? 0))
            };
        }

        public bool AddToCart(int studentId, int courseId)
        {
            var exists = _context.Carts.Any(c => c.StudentId == studentId && c.CourseId == courseId);
            if (exists) return true;
            var enrolled = _context.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
            if (enrolled) return false;
            _context.Carts.Add(new Cart { StudentId = studentId, CourseId = courseId });
            _context.SaveChanges();
            return true;
        }

        public List<CourseCardVM> GetCartItems(int studentId)
        {
            return _context.Carts
                .Where(c => c.StudentId == studentId)
                .Include(c => c.Course)
                    .ThenInclude(co => co.CourseDetails)
                .Include(c => c.Course)
                    .ThenInclude(co => co.courseCategory)
                .Select(c => new CourseCardVM
                {
                    courseId = c.CourseId,
                    Title = c.Course.Title,
                    SubTitle = (c.Course.CourseDetails.Description ?? "").Split('.', StringSplitOptions.None).FirstOrDefault() ?? string.Empty,
                    CategoryName = c.Course.courseCategory.Name,
                    Total_Price = c.Course.CourseDetails.Total_Price,
                    Actual_Price = c.Course.CourseDetails.Actual_Price,
                    Discount_Percent = c.Course.CourseDetails.Discount_Percent,
                    Thumbnail_Url = c.Course.CourseDetails.Thumbnail_Url
                }).ToList();
        }

        public void RemoveFromCart(int studentId, int courseId)
        {
            var item = _context.Carts.FirstOrDefault(c => c.StudentId == studentId && c.CourseId == courseId);
            if (item != null)
            {
                _context.Carts.Remove(item);
                _context.SaveChanges();
            }
        }

        public int GetCartCount(int studentId)
        {
            return _context.Carts.Count(c => c.StudentId == studentId);
        }
    }
}
