# 1. Feature Overview

The Instructor "My Courses" page includes a fully functional, server-side search and filtering system. It allows instructors to quickly locate their courses using three main criteria:

*   **Search by course title:** A text input for keyword matching.
*   **Category filter:** A dropdown to narrow courses by topics like "Software Development" or "Data Science".
*   **Status filter:** A dropdown to view courses by their lifecycle state, such as "Published", "Draft", or "PendingReview".

All three filters work in tandem. An instructor can search for "React", filter by the "Software Development" category, and further narrow the results to only "Published" courses simultaneously.

# 2. UI Request Flow

The filtering process follows a strict unidirectional flow from the frontend UI to the backend database and back.

**Flow:**
Search Input / Dropdown Select
↓
Filter Form Submit (GET Request)
↓
Controller Action (`MyCourses`)
↓
Service Method (`CourseManagementService`)
↓
Filtered Query (`IQueryable` chaining)
↓
Updated Course List returned to View

**In Practice:**
When the user types a keyword or selects a dropdown option, the HTML `<form>` submits a GET request. The backend receives these parameters, filters the database records, and returns a freshly rendered list of courses that match the criteria.

# 3. How Search Keyword Is Sent To Backend

The system relies on standard HTTP GET requests using query strings. This means the filter state is embedded directly in the URL, making it bookmarkable and transparent.

**Form Submission & Binding:**
The UI uses a standard HTML form with `method="get"`. Each input has a `name` attribute (`search`, `category`, `status`). When submitted, the browser constructs a URL with these parameters.

**Practical Query String Flow:**

*   Keyword only:
    `/Instructor/Home/MyCourses?search=react`

*   Keyword + Category:
    `/Instructor/Home/MyCourses?search=react&category=Software Development`

*   All combined:
    `/Instructor/Home/MyCourses?search=react&category=Software Development&status=Published`

**Persistence:**
The controller receives these query string parameters and passes them back to the view using `ViewBag` (e.g., `ViewBag.SearchTerm = search`). The Razor view uses these ViewBag values to set the `value` or `selected` attributes on the inputs, ensuring the filters don't visually reset after the page reloads.

# 4. Combined Filter Logic

The system is designed to handle any combination of missing or present filters gracefully.

**Conditions Working Together:**
*   **Only Keyword:** If only `search` has a value, the backend filters courses where the title contains the keyword. Category and Status checks are skipped.
*   **Only Category:** If only `category` is selected, the backend returns all courses matching that category, ignoring title and status.
*   **All Combined:** If `search`, `category`, and `status` all have values, the backend applies an `AND` condition. A course must match the keyword *AND* the category *AND* the status to be returned.

The logic flows linearly: start with all courses for the instructor, then sequentially apply filters only if their corresponding parameter is not null or empty.

# 5. Controller Responsibility

The `HomeController` in the Instructor area acts purely as an orchestrator.

**What it does:**
1.  Receives the incoming filter parameters (`search`, `category`, `status`).
2.  Passes these parameters directly to the `ICourseManagementService`.
3.  Populates the `ViewBag` with the current filter state.
4.  Returns the View with the filtered data.

**Why stay thin?**
Controllers should not handle `IQueryable` logic, string matching, or database context calls. Keeping the controller thin ensures that the exact same filtering logic can be reused by an API endpoint later without rewriting the code.

# 6. Service Layer Flow

The business logic resides entirely within the `CourseManagementService`.

**Flow:**
1.  The service method receives the instructor ID and the optional filter strings.
2.  It initializes a base query against the database context (e.g., all courses belonging to this instructor).
3.  It conditionally appends `Where()` clauses based on which filter parameters were provided.
4.  It projects the final filtered dataset into a lightweight ViewModel (`MyCourseVM`) and returns the list.

# 7. LINQ Filtering Flow

The core of the server-side filtering relies on Entity Framework Core and `IQueryable` chaining. This is where the actual conditions are applied.

**Practical Logic:**
```csharp
// 1. Initialize base query (Not executed yet)
var query = _context.Courses.Where(c => c.instructor_id == instructorId);

// 2. Conditionally chain filters
if (!string.IsNullOrWhiteSpace(search)) {
    query = query.Where(c => c.Title.Contains(search));
}

if (!string.IsNullOrWhiteSpace(category)) {
    query = query.Where(c => c.courseCategory.Name == category);
}

// 3. Project and Execute
return query.Select(c => new MyCourseVM { ... }).ToList();
```

**Why this is efficient:**
Because `query` is an `IQueryable`, the `.Where()` clauses do not execute immediately. They build up an expression tree. The actual SQL query is only sent to the database when `.ToList()` is called at the very end. The database engine does the heavy filtering, not the C# web server.

# 8. Reusing Existing Search Logic

The Instructor module follows the same pattern used in the Student module and the public Landing page.

**Consistency & Maintainability:**
By relying on `IQueryable` chaining and GET parameters across all modules, we ensure the application behaves predictably. 

**DRY Principle:**
While the Instructor search operates on the `CourseManagementService` (since instructors only see their own courses), the underlying architectural pattern is identical to the public search. If we decide to move to a specialized Search/Filter DTO in the future, the pattern allows us to upgrade all areas uniformly.

# 9. Filter Parameter Structure

Currently, the system accepts individual parameters (`string? search`, `string? category`, `string? status`). 

If the filtering requirements grow in the future (e.g., adding sort order, date ranges, or pagination), these parameters should be wrapped in a single ViewModel:

**Example Future Structure:**
```csharp
public class CourseFilterVM 
{
    public string? SearchKeyword { get; set; } // Matches course title
    public string? CategoryName { get; set; }  // Matches category relationship
    public string? Status { get; set; }        // Matches course lifecycle state
}
```
This structure keeps the controller method signature clean, even as filters expand.

# 10. Performance Notes

*   **IQueryable Deferred Execution:** As mentioned, filters are chained before execution. This prevents pulling thousands of records into server RAM just to filter them locally.
*   **Database-level Filtering:** The `Contains` and exact match logic translate directly into optimized `LIKE` and `=` SQL statements.
*   **Projection:** By using `.Select(c => new MyCourseVM {...})`, Entity Framework only retrieves the specific columns needed for the UI, ignoring heavy fields like long descriptions or raw binary data.

# 11. Final Summary

The Instructor "My Courses" search and filter system is built on a clean, scalable, and maintainable architecture. By utilizing HTTP GET parameters, thin controllers, and deferred `IQueryable` execution in the service layer, the system provides real-time, accurate, and highly performant filtering without overly burdening the web server or duplicating logic across the application.