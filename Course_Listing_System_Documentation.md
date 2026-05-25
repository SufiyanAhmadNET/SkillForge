# Course Listing System Documentation - SkillForge

This document provides a comprehensive technical overview of the course discovery architecture, rendering logic, and dynamic grouping implemented across the Landing Page and Courses portals.

---

## 1. ARCHITECTURE OVERVIEW

The system is built on a **Shared Logic Model**. Instead of having separate backend endpoints for guests and students, we use a single `ICourseQueryService` that provides raw data, while the **Razor Rendering Layer** handles the specific grouping, sorting, and conditional display logic.

### Core Objectives:
-   **Eliminate Empty Space**: Prevent half-empty carousels/sliders.
-   **Highlight Freshness**: Always show "Recently Added" courses first.
-   **Unified UI**: Use the same course card component everywhere.
-   **Dynamic Grouping**: Automatically merge small categories into a mixed "Explore More" section.

---

## 2. DATA PROCESSING FLOW (THE LINQ LOGIC)

The system follows a strict sequence to transform a flat list of courses into a structured UI.

### Step 1: Gather & Unique Filter
We gather all courses from the ViewModel and ensure no duplicates exist using `GroupBy(c => c.courseId)`.

### Step 2: "Recently Added" Extraction
We sort by the primary key descending to find the newest launches.
```csharp
var recentlyAdded = allCourses
    .OrderByDescending(c => c.courseId)
    .Take(8)
    .ToList();
```

### Step 3: Category Filtering (The Threshold Logic)
To maintain visual quality, we only create dedicated carousel rows for "Large Categories" (those with 3 or more courses).
```csharp
var validCategories = Model.CategorySections
    .Where(c => c.Courses != null && c.Courses.Count >= 3)
    .OrderByDescending(c => c.Courses.Count)
    .ToList();
```

### Step 4: Mixed Section Generation (Leftover Logic)
Any category that has courses but fails the "3+ threshold" is identified and its courses are merged into a single collection.
```csharp
var mixedCourses = Model.CategorySections
    .Where(c => c.Courses != null && c.Courses.Count > 0 && c.Courses.Count < 3)
    .SelectMany(c => c.Courses)
    .GroupBy(c => c.courseId) // Ensure unique courses
    .Select(g => g.First())
    .ToList();
```

---

## 3. LANDING PAGE RENDERING LOGIC

The landing page (`Index.cshtml`) is a curated preview designed for conversion.

-   **Priority 1: Recently Added**: Shows the top 8 latest courses in a slider.
-   **Priority 2: Top Categories**: Limited to the **Top 2** valid categories (Count >= 3).
-   **No Mixed Grid**: Small categories are omitted from the landing page to keep it clean and high-impact.
-   **Global CTA**: An "Explore All Courses" button directs users to the full catalog.

---

## 4. FULL COURSES PAGE LOGIC (GUEST & STUDENT)

The full discovery page (`Courses.cshtml`) uses a tiered layout to display **every** published course.

1.  **Top Tier: Recently Added Slider**: Captures immediate interest.
2.  **Middle Tier: Full Category Sliders**: Only for categories with 3+ courses.
3.  **Bottom Tier: "Explore More" Grid**: 
    -   Displays courses from all "Small Categories" (< 3 courses).
    -   **Rendering Choice**: Uses a **Responsive Grid** instead of a slider. This prevents layout breaks if there are only 1 or 2 courses.

---

## 5. UI/UX & RESPONSIVE DECISIONS

### Slider vs. Grid
-   **Slider (`.course-row`)**: Used for Large Categories. It allows horizontal exploration without taking up massive vertical space.
-   **Grid (`.course-grid`)**: Used for the Mixed Section. Each card is locked to **340px width** (matching the sliders) but wraps to new lines.
    -   **Desktop**: 3 cards per row.
    -   **Tablet**: 2 cards per row.
    -   **Mobile**: 1 card per row (Full width).

### Alignment Consistency
The system uses a **Breakout Pattern** (`.category-section`). This allows the background tint to stretch full-width while the content remains locked to a **1216px mathematical grid**, ensuring section titles and cards align perfectly.

---

## 6. SHARED COMPONENTS & SECURITY

### `_CourseCardPartial.cshtml`
This is the single source of truth for course rendering. It uses conditional logic to handle routing:
-   **Authenticated**: Links to `/User/Home/CourseDetails`.
-   **Guest**: Links to `/Pubic/Home/CoursesDetails`.

### Wishlist & Guest Interaction
-   **Heart Icon**: Shared UI component across all pages.
-   **Protection**: JavaScript (`site.js`) checks `isAuthenticated` before triggering AJAX.
-   **Guest Behavior**: Shows a "Please login" alert instead of performing the action, preventing unauthorized state changes.

---

## 7. DEVELOPER REFERENCE

| Logic Point | File / Location |
| :--- | :--- |
| Dynamic Filtering | `Areas/Pubic/Views/Home/Courses.cshtml` (Razor Block) |
| Grid Sizing | `wwwroot/css/courses.css` (`.course-grid-item`) |
| Breakout Layout | `wwwroot/css/courses.css` (`.category-section`) |
| Guest Restrictions | `wwwroot/js/site.js` (`requireLogin`) |

---
*Last Updated: May 2026 - Finalized Course Listing System*
