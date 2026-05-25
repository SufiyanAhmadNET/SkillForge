using Microsoft.EntityFrameworkCore;
using SkillForge.Data;
using SkillForge.Interfaces;
using SkillForge.Models;
using SkillForge.Services.Courses.Models;

namespace SkillForge.Services.Courses
{
    // Search service implementation
    public class SearchService : ISearchService
    {
        private readonly SkillForgeDbContext _context;

        public SearchService(SkillForgeDbContext context)
        {
            _context = context;
        }

        public SearchResultVM SearchCourses(string keyword, int studentId = 0)
        {
            var result = new SearchResultVM { Keyword = keyword };

            if (string.IsNullOrWhiteSpace(keyword)) return result;

            var searchTerm = keyword.ToLower().Trim();

            // Fetch wishlisted courses for the student
            var wishlistedIds = studentId > 0
                ? _context.Wishlists.AsNoTracking()
                    .Where(w => w.StudentId == studentId)
                    .Select(w => w.CourseId).ToList()
                : new List<int>();

            // Fetch enrolled course IDs if studentId > 0
            var enrolledIds = studentId > 0
                ? _context.Enrollments.AsNoTracking()
                    .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
                    .Select(e => e.CourseId).ToList()
                : new List<int>();

            // 1. Search Logic: Filter approved/published courses
            var query = _context.Courses.AsNoTracking()
                .Where(c => c.Status == CourseStatus.Approved || c.Status == CourseStatus.Published)
                .Include(c => c.CourseDetails)
                .Include(c => c.courseCategory)
                .AsQueryable();

            // Filter by keyword
            var matchedCourses = query.Where(c =>
                c.Title.ToLower().Contains(searchTerm) ||
                (c.CourseDetails != null && c.CourseDetails.ShortSummary != null && c.CourseDetails.ShortSummary.ToLower().Contains(searchTerm)) ||
                (c.CourseDetails != null && c.CourseDetails.Description != null && c.CourseDetails.Description.ToLower().Contains(searchTerm)) ||
                (c.courseCategory != null && c.courseCategory.Name.ToLower().Contains(searchTerm))
            ).ToList();

            if (!matchedCourses.Any()) return result;

            // 2. Exact Matches (prioritize enrolled courses if logged in)
            result.ExactMatches = matchedCourses
                .OrderByDescending(c => enrolledIds.Contains(c.Id))
                .Take(8)
                .Select(c => MapToCard(c, wishlistedIds))
                .ToList();

            // 3. Related Courses (same categories as matches, exclude matches)
            var matchedCategoryIds = matchedCourses.Select(c => c.category_id).Distinct().ToList();
            var matchedCourseIds = matchedCourses.Select(c => c.Id).ToList();

            result.RelatedCourses = query
                .Where(c => matchedCategoryIds.Contains(c.category_id) && !matchedCourseIds.Contains(c.Id))
                .Take(6)
                .Select(c => MapToCard(c, wishlistedIds))
                .ToList();

            return result;
        }

        // Map Course entity to CourseCardVM
        private static CourseCardVM MapToCard(Course c, List<int> wishlistedIds)
        {
            return new CourseCardVM
            {
                courseId = c.Id,
                Title = c.Title,
                SubTitle = c.CourseDetails?.ShortSummary ?? c.CourseDetails?.Description,
                ShortSummary = c.CourseDetails?.ShortSummary ?? c.CourseDetails?.Description,
                CategoryName = c.courseCategory?.Name ?? "Uncategorized",
                Difficulty = c.CourseDetails?.Difficulty.ToString() ?? "None",
                Total_Price = c.CourseDetails?.Total_Price ?? 0,
                Actual_Price = c.CourseDetails?.Actual_Price ?? 0,
                Discount_Percent = c.CourseDetails?.Discount_Percent ?? 0,
                Thumbnail_Url = c.CourseDetails?.Thumbnail_Url,
                IsWishListed = wishlistedIds.Contains(c.Id)
            };
        }
    }
}
