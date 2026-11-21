# API Contract (MVP)

All routes are prefixed with `/api`. Protected routes require `Authorization: Bearer <accessToken>`. Project responses include `canManage`, indicating whether the current user can manage that project.

## Auth
- `POST /auth/register` — `{ name, email, password }` → `{ accessToken, expiresAt, user }`.
- `POST /auth/login` — `{ email, password }` → `{ accessToken, expiresAt, user }`.
- `GET /auth/me` — current user profile.

## Projects
- `GET /projects` — projects visible to current user.
- `POST /projects` — Global Admin/Manager; `{ name, description, startDate?, dueDate? }`.
- `GET /projects/{id}` — project details; requires membership unless Admin.
- `PUT /projects/{id}` — Global Admin or project Manager.
- `POST /projects/{id}/members` — Global Admin or project Manager; `{ email, role: "Manager"|"Member" }`.
- `GET /projects/{id}/members` — project members.

## Tasks
- `GET /projects/{id}/tasks` — project members; optional `status`, `assigneeId` query filters.
- `POST /projects/{id}/tasks` — Global Admin or project Manager.
- `GET /tasks/{id}` — project members.
- `PUT /tasks/{id}` — Global Admin or project Manager.
- `DELETE /tasks/{id}` — Global Admin or project Manager.
- `PATCH /tasks/{id}/status` — project Manager/Admin or the assigned Member.

## Comments
- `GET /tasks/{id}/comments` — project members.
- `POST /tasks/{id}/comments` — project members; `{ body }`.

## Administration and initial setup
- `POST /setup/bootstrap-admin` — one-time bootstrap protected by `BOOTSTRAP_ADMIN_TOKEN`; creates the first Admin only when no Admin exists.
- `GET /admin/users` — Admin only.
- `PATCH /admin/users/{id}/role` — Admin only; `{ role: "Admin"|"Manager"|"Member" }`.

## Dashboard
- `GET /dashboard/summary` — `{ projectCount, openTaskCount, completedTaskCount, overdueTaskCount, statusBreakdown, priorityBreakdown, recentTasks }`.

## Error shape
Unexpected errors use RFC 7807 Problem Details. Validation errors return 400 with field-level errors. Missing/invalid authentication returns 401. Authenticated but unauthorized requests return 403 or 404 where resource existence must not be disclosed.
