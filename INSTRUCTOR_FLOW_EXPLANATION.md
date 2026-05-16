# Instructor Onboarding & Mentor Application Flow

This document explains the implementation of the instructor verification system in SkillForge.

## 1. Goal (The "Why")
To maintain platform quality, we moved from an "Open Access" model to a "Verified Mentor" model. This ensures that only instructors with a proven background can create and publish courses, while still allowing new instructors to register and build their professional profile.

## 2. Core Components (The "What")

### A. Data Model Extensions
- **InstructorProfile**: Added `AboutYou`, `CurrentRole`, `Expertise`, and `YearsExperience` to create a professional identity.
- **MentorApplication**: A new table to store application-specific data (Resume, Why Teach, Topics) and the **Approval Status** (`NotApplied`, `Pending`, `Approved`, `Rejected`).

### B. Access Control (The Gatekeeper)
We implemented a centralized restriction logic in `InstructorBaseController`.

### C. Dashboard Experience
Modified the dashboard to act as an onboarding hub. Approved instructors see stats; non-approved instructors see a "Teach on SkillForge" Call-to-Action (CTA).

---

## 3. Logic & Code Explanation (The "How")

### I. Centralized Restriction Logic
**File:** `Areas/Instructor/Controllers/InstructorBaseController.cs`

```csharp
// Intercepts every action in the instructor area
public override void OnActionExecuting(ActionExecutingContext context)
{
    // 1. Identify the instructor and their latest application status
    var status = application?.Status ?? MentorApplicationStatus.NotApplied;
    ViewBag.ApplicationStatus = status;

    // 2. Define features that require approval
    var restrictedActions = new[] { "AddCourse", "MyCourses", "CourseDetails", "EditCourse", "DeleteCourse", "Earning" };

    // 3. The Restriction Logic
    if (status != MentorApplicationStatus.Approved && restrictedActions.Contains(actionName))
    {
        // Redirect non-approved users to the application tab with a warning
        context.Result = new RedirectToActionResult("Profile", "Home", new { tab = "application" });
    }
}
```
**Why this works:** Instead of adding `if` checks in every single action, we use the `BaseController` to handle security globally.

### II. Dynamic Tab Handling (URL Parameters)
**File:** `Areas/Instructor/Views/Home/Profile.cshtml`

We use `URLSearchParams` to ensure that when a user is redirected (e.g., from a blocked page), they land on the correct tab automatically.

```javascript
// URL: .../Profile?tab=application
const urlParams = new URLSearchParams(window.location.search);

if (urlParams.get('tab') === 'application') {
    // Automatically triggers the "click" event on the Application sidebar button
    const appBtn = document.querySelector('[data-section="applicationSection"]');
    if (appBtn) appBtn.click();
}
```
**Concept:** This improves UX by "remembering" which section the user needs to see after a redirect.

### III. State-Based Dashboard UI
**File:** `Areas/Instructor/Views/Home/Dashboard.cshtml`

We use conditional Razor logic to change the entire layout based on the `ApplicationStatus`.

```razor
@if (Model.ApplicationStatus != MentorApplicationStatus.Approved)
{
    <!-- Show "Become a Mentor" CTA Card -->
}
else
{
    <!-- Show Full Stats (Earnings, Student Count, Star Ratings) -->
}
```
**Why this works:** It guides the user toward the next step (Applying) rather than showing empty/confusing data.

### IV. Professional Auto-Sync
**File:** `HomeController.cs` -> `ApplyMentor`

We don't ask the instructor for their "Role" or "Experience" again in the application form. Instead, the application model is saved, and we view it alongside the profile.

```csharp
// When applying, we only ask for specific application questions
application.InstructorId = instructorId;
application.Status = MentorApplicationStatus.Pending; // Force pending state
_context.MentorApplications.Add(application);
```
**Logic:** This keeps the data structure clean and reduces "form fatigue" for the instructor.

---

## 4. Summary of Status Flow
1. **Not Applied**: Sees "Apply as Mentor" CTA.
2. **Pending**: Sidebar links hidden; sees "Under Review" alert. Cannot create courses.
3. **Approved**: Full access unlocked. Dashboard stats appear. Sidebar links visible.
4. **Rejected**: Sees feedback from admin and a "Reapply" button.
