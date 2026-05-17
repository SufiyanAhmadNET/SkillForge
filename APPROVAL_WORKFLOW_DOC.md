# SkillForge Approval Workflows: Developer Documentation

This document provides a comprehensive guide to the **Mentor Application** and **Course Approval** workflows within SkillForge. It explains the MVC flow, service-layer logic, and the LINQ queries that power these features.

---

## 1. Mentor Application Workflow

### Overview
Before an instructor can create courses, they must apply to become a verified "Mentor." This prevents spam and ensures high-quality content.

### Step-by-Step Flow
1.  **Request:** Instructor completes the "Mentor Application" form in their profile.
2.  **Controller:** `Areas/Instructor/Controllers/HomeController.cs` -> `ApplyMentor` action.
3.  **Validation:** The system checks if the instructor's profile (Role, Experience, Skills) is complete before allowing submission.
4.  **Database:** A new record is added to the `MentorApplications` table with `Status = Pending`.
5.  **Restriction:** The `InstructorBaseController` intercepts subsequent requests and blocks access to course management until the status is `Approved`.

### Important Code: Submission Logic
**File:** `Areas/Instructor/Controllers/HomeController.cs`

```csharp
[HttpPost]
public IActionResult ApplyMentor(MentorApplication application, IFormFile ResumeFile)
{
    // 1. Get Logged-in Instructor ID from Claims
    var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    int instructorId = int.Parse(idClaim);

    // 2. Map file to path using MediaService
    if (ResumeFile != null) 
    {
        application.ResumePath = _mediaService.UploadResume(ResumeFile);
    }

    // 3. Set Initial State
    application.InstructorId = instructorId;
    application.Status = MentorApplicationStatus.Pending; // Enum: 0=Pending, 1=Approved, 2=Rejected
    application.CreatedAt = DateTime.UtcNow;

    // 4. Save to Database
    _context.MentorApplications.Add(application);
    _context.SaveChanges();

    return RedirectToAction("Profile");
}
```

---

## 2. Admin Approval: Mentor Applications

### Overview
Admins review pending applications and either Approve or Reject them.

### Step-by-Step Flow
1.  **Listing:** Admin opens "Pending Approvals."
2.  **Service:** `AdminService.GetAllMentorApplications()` fetches all records.
3.  **LINQ Query:** Filters and joins data for a clean view.
4.  **Action:** Admin clicks "Approve" or "Reject."
5.  **Update:** `UpdateApplicationStatus` updates the database and unlocks features for the instructor.

### Important LINQ: Fetching Applications
**File:** `Services/Admin/AdminService.cs`

```csharp
public List<MentorApplicationListVM> GetAllMentorApplications()
{
    return _context.MentorApplications
        .Include(m => m.Instructor)
        .ThenInclude(i => i.Profile) // Join with Instructor and their Profile
        .Select(m => new MentorApplicationListVM
        {
            ApplicationId = m.Id,
            InstructorName = m.Instructor.Profile.FirstName + " " + m.Instructor.Profile.LastName,
            Email = m.Instructor.Email,
            Status = m.Status, // Enum value
            ResumePath = m.ResumePath,
            AppliedDate = m.CreatedAt
        })
        .OrderByDescending(m => m.AppliedDate) // Show newest first
        .ToList();
}
```
**Logic Explanation:**
- `Include`: Performs an **Eager Load** (SQL JOIN) to get instructor details in a single query.
- `Select`: Projects the database entity into a **ViewModel (`MentorApplicationListVM`)**, ensuring only necessary data is sent to the View.

---

## 3. Course Submission & Admin Approval

### Overview
Instructors create courses in a `Draft` state. Once ready, they "Submit for Review," making the course visible to admins but not yet to students.

### The "Auto-Reset" Security Logic
To prevent instructors from sneaking in bad content *after* a course is approved, any significant edit (editing details or changing the syllabus) automatically resets the course status back to `Draft`.

### Step-by-Step Flow
1.  **Instructor Action:** Clicks "Submit for Review" or saves an edit.
2.  **Service Logic:** `CourseManagementService` updates the status.
3.  **Admin Review:** Course appears in `Admin/Home/Courses`.
4.  **Admin Action:** Admin previews the course (Read-Only) and chooses Approve/Reject.

### Important Code: Status Management
**File:** `Services/Courses/CourseManagementService.cs`

```csharp
public CourseReturn UpdateCourse(..., string submitAction)
{
    var course = _context.Courses.Find(courseVM.Id);

    // 1. Determine Status based on Action
    if (submitAction == "submit")
        course.Status = CourseStatus.PendingReview;
    else if (course.Status == CourseStatus.Approved)
        course.Status = CourseStatus.Draft; // Security: Reset to draft if edited after approval

    course.UpdatedAt = DateTime.UtcNow;
    _context.SaveChanges();
}
```

### Important LINQ: Admin Course Review Listing
**File:** `Services/Admin/AdminService.cs`

```csharp
public List<AdminCourseReviewVM> GetAllCoursesForReview()
{
    return _context.Courses
        .Include(c => c.courseCategory)
        .Select(c => new AdminCourseReviewVM
        {
            Id = c.Id,
            Title = c.Title,
            Status = c.Status,
            SubmittedDate = c.UpdatedAt,
            // Sub-query to count lessons across all modules
            LessonCount = _context.CourseModules
                                  .Where(m => m.CourseId == c.Id)
                                  .SelectMany(m => m.Lessons).Count()
        })
        .OrderByDescending(c => c.Status == CourseStatus.PendingReview) // Prioritize pending
        .ThenByDescending(c => c.SubmittedDate)
        .ToList();
}
```
**LINQ Deep Dive:**
- `SelectMany`: Flattens the collection of lessons across multiple modules to get a total count.
- `OrderByDescending(bool)`: In LINQ, `true` (1) comes before `false` (0) in descending order, effectively "pinning" pending requests to the top of the list.

---

## 4. Summary of Status Enums

### Mentor Application Status
| Value | Name | Description |
| :--- | :--- | :--- |
| 0 | `Pending` | Application submitted, waiting for admin. |
| 1 | `Approved` | Instructor is now a Mentor. Course creation unlocked. |
| 2 | `Rejected` | Application denied. Admin feedback is shown. |

### Course Status
| Value | Name | Description |
| :--- | :--- | :--- |
| 0 | `Draft` | Course is private to instructor. |
| 1 | `PendingReview`| Visible to admin for approval. |
| 2 | `Approved` | Course is live/public for students. |
| 3 | `Rejected` | Admin requested changes. |

---

## 5. Key Architecture Components

- **Controllers:** Handle user input and routing (e.g., `Areas/Admin/Controllers/HomeController.cs`).
- **Services:** Contain the "Business Logic" and LINQ queries to keep controllers thin (e.g., `AdminService.cs`).
- **ViewModels (VM):** Specially designed classes to carry only the data needed for a specific page (e.g., `AdminCourseReviewVM.cs`).
- **Entities:** Classes that map directly to Database Tables (e.g., `Course.cs`).
