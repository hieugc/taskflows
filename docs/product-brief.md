# Product Brief

## Product
TaskFlow is a lightweight team workspace for organizing projects, assigning work, discussing tasks, and monitoring delivery.

## Primary users
- **Admin**: manages platform-level access and can access all projects.
- **Manager**: creates and manages projects, manages project members, and coordinates tasks.
- **Member**: views projects they belong to, updates tasks assigned to them, and participates in task discussions.

## Problem and outcome
Teams need one place to understand what work exists, who owns it, what is blocked or overdue, and what has recently changed. The MVP provides a clear project/task workflow and a compact operational dashboard.

## MVP scope
- Registration, login, current-user profile.
- Global roles and project-level membership.
- Project create/read/update/archive.
- Task create/read/update/status change/assignment.
- Comments on tasks.
- Dashboard totals and status/priority breakdowns.
- Responsive desktop and mobile web UI.

## Non-goals for MVP
- Multi-tenant billing, subscriptions, external SSO, file attachments, realtime collaboration, email/push notifications, time tracking, Gantt charts, custom workflow builders, and production hosting.

## Assumptions
- A single organization/workspace is sufficient for the first release; no tenant isolation model was requested.
- Public registration is enabled and always grants the least-privileged global role (`Member`).
- A project has a manager/owner and explicitly enrolled members.
- All timestamps are stored in UTC and displayed in the browser's local timezone.
