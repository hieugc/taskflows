# Architecture

## Decision
Use a **modular monolith** for the MVP. It minimizes operational overhead while keeping clear module boundaries. Microservices are not justified by the supplied scope or scale assumptions.

## Containers
- **Web SPA**: React + TypeScript + Vite. Handles navigation, forms, dashboards, and client-side presentation.
- **API**: ASP.NET Core 10 Web API. Owns validation, authentication, authorization, business rules, and data access.
- **Database**: PostgreSQL. Stores users, projects, memberships, tasks, and comments.

## Backend module boundaries
- `Auth`: registration, login, token issuance, current-user profile.
- `Projects`: project lifecycle, membership, project access rules.
- `Tasks`: task lifecycle, assignment, status and priority transitions.
- `Comments`: discussion on tasks, scoped to project membership.
- `Analytics`: dashboard aggregates scoped by authorization.

## Dependency direction
Controllers -> application services/access policies -> EF Core DbContext -> PostgreSQL. Domain entities and policies do not depend on HTTP or persistence frameworks. Frontend calls only the API; it never accesses the database directly.

## Authorization model
- Global role: `Admin`, `Manager`, `Member`.
- Project role: `Manager`, `Member` stored in `ProjectMembers`.
- Admin bypasses project membership checks.
- Global Manager can create projects; management of an existing project requires project-level Manager membership (or Admin).
- Project members can read project data and comment.
- Project Manager/Admin can create and edit tasks. A Member can change status only on a task assigned to them.
- Deny by default; every project/task/comment query must be scoped through project access.

## Data model
`Users 1..* ProjectMembers *..1 Projects`; `Projects 1..* WorkTasks`; `Users 0..* WorkTasks` as assignees; `WorkTasks 1..* Comments`; `Users 1..* Comments` as authors.

## API conventions
- Base path: `/api`.
- JSON camelCase; UTC ISO-8601 timestamps.
- Protected routes require a valid bearer token.
- List endpoints return arrays in MVP; pagination is a next-release requirement before large datasets.
- Authorization failures use 403 or a non-disclosing 404 for inaccessible project resources.

## Key decisions
1. Modular monolith over microservices: lower operational cost and simpler transactions.
2. EF Core + Npgsql: conventional .NET/PostgreSQL integration.
3. Global role + project membership: supports platform-level and project-level access without conflating them.
4. JWT access token for SPA MVP: simple local setup; production should add rotating refresh tokens in Secure HttpOnly cookies.

## Risks and future evolution
- Add workspace/tenant IDs and tenant-scoped uniqueness before supporting multiple organizations.
- Add pagination, query limits, and indexes tuned from query plans before high-volume use.
- Add refresh-token rotation/revocation, rate limiting, email verification, password reset, audit log retention, and security monitoring before production exposure.
