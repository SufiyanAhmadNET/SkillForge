PROJECT CONTEXT (SkillForge):

- This is an ASP.NET Core MVC application
- Architecture is layered (Controller → Service → Data)
- No business logic inside Controllers
- Controllers only handle request/response

---

CORE MODULES (STRICT BOUNDARIES):

1. User Module
   - Handles student/user operations
   - Course browsing, enrollment, dashboard

2. Instructor Module
   - Course creation, update, management
   - Owns course-related operations

3. Admin Module
   - Platform control (users, courses, approvals)
   - Full access, but MUST NOT duplicate logic

---

AUTHENTICATION & AUTHORIZATION:

- Custom authentication system is used (NOT default Identity unless already present)
- Role-based access:
  - User
  - Instructor
  - Admin

RULES:
- NEVER break existing auth flow
- NEVER replace auth system
- Only extend if needed
- Respect role checks already implemented

---

SERVICE LAYER RULES:

- All business logic must be in Services
- Controllers must call Services only
- Do NOT move logic into Controllers
- Do NOT access DB directly from Controllers

---

DATA ACCESS RULES:

- Follow existing pattern (LINQ / EF Core / Queries)
- Do NOT randomly change data access strategy
- If switching logic (e.g., LINQ → SQL), preserve method contracts

---

MODIFICATION RULES (VERY STRICT):

- Do NOT rewrite full files unless necessary
- Do NOT break existing flow
- Make incremental changes only
- Preserve naming conventions and structure
- Reuse existing methods instead of duplicating logic

---

CODE STYLE:

- Match existing coding style exactly
- Keep methods small and readable
- Use short, human-like comments (no explanations)
- No unnecessary comments or verbose text

---

UI / RAZOR RULES:

- Do NOT break layout or structure
- Only adjust classes/styles where needed
- Preserve existing HTML hierarchy

---

ERROR HANDLING:

- Fix root cause, not symptoms
- Do NOT add hacks or temporary fixes

---

BEHAVIOR:

- Think like a maintainer, not a redesign architect
- Infer patterns from existing code before writing new code
- If unsure, follow existing implementation style
- Avoid overengineering

---

WORKFLOW:

1. Understand existing code
2. Identify minimal change required
3. Apply fix/enhancement
4. Do NOT introduce side effects