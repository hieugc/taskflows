# Requirements and Acceptance Criteria

## Functional requirements

### FR-001 Authentication
- Users can register with name, email, and password.
- Users can log in and retrieve their own profile.
- Public registration always creates a `Member` role.
- Invalid credentials return a generic unauthorized response.

### FR-002 Global RBAC
- Global roles are `Admin`, `Manager`, and `Member`.
- Admin can access all projects and perform all project/task operations.
- Manager can create projects and manage projects where they are a project manager.
- Member can access only projects where they have a membership.
- API authorization is enforced server-side.

### FR-003 Projects
- A manager/admin can create a project with name, description, and optional dates.
- The creator is added as a project manager.
- A project manager/admin can update or archive a project.
- Project members can list and view projects they belong to.

### FR-004 Tasks
- Project managers/admins can create, assign, and edit tasks in their projects.
- Project members can view project tasks.
- A member can update status only for a task assigned to them.
- Task fields include title, description, status, priority, assignee, due date, and timestamps.
- Status values: `Todo`, `InProgress`, `InReview`, `Done`, `Blocked`.
- Priority values: `Low`, `Medium`, `High`, `Urgent`.

### FR-005 Comments
- Project members can read and add comments to tasks in their projects.
- A comment records its author and creation time.

### FR-006 Dashboard and analytics
- The dashboard shows visible project count, open tasks, completed tasks, overdue tasks, task status breakdown, and recent task activity.
- Analytics are scoped to projects the current user is allowed to see, except Admin who can see all.

## Non-functional requirements
- NFR-001: API uses ASP.NET Core 10 and PostgreSQL.
- NFR-002: Frontend uses React and TypeScript and adapts to mobile/tablet/desktop.
- NFR-003: Passwords are hashed; plaintext passwords are never stored or logged.
- NFR-004: Access tokens are signed and expire; signing key comes from environment configuration.
- NFR-005: Database constraints enforce foreign keys and membership uniqueness.
- NFR-006: API returns consistent Problem Details for unexpected errors.
- NFR-007: Inputs are validated at API boundaries.

## Acceptance criteria
- AC-001: Given a new visitor, when they register, then the account is created with global role `Member`.
- AC-002: Given a member without project membership, when they request a project, then the API returns 404/forbidden without disclosing project details.
- AC-003: Given a member assigned to a task, when they change its status, then the status is persisted.
- AC-004: Given a member not assigned to a task, when they attempt to edit its title or assignment, then the API denies the request.
- AC-005: Given a project manager, when they create a task, then the task belongs to that project and appears in the project task list.
- AC-006: Given comments on a task, when a project member opens the task, then comments include author and timestamp.
- AC-007: Given multiple projects with mixed task statuses and due dates, when the dashboard loads, then counts reflect only visible projects and overdue tasks exclude completed tasks.
- AC-008: Given an unauthenticated request to a protected endpoint, then the API returns 401.
