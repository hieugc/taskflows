# Implementation Status

| ID | Package | Status | Evidence / limitation |
|---|---|---|---|
| TF-001 | Product brief and requirements | Implemented | `product-brief.md`, `requirements.md` |
| TF-002 | Architecture and data/API model | Implemented | `architecture.md`, `api-contract.md`, `authorization-matrix.md` |
| TF-003 | PostgreSQL schema and Docker Compose | Implemented | `db/init/001-schema.sql`, `docker-compose.yml`; schema exercised by PostgreSQL Testcontainers integration suite |
| TF-004 | .NET API foundation | Implemented, CI-verified | Self-hosted .NET 10 Release build |
| TF-005 | Authentication and global RBAC | Implemented, integration-tested | Registration role restriction, login, token role freshness |
| TF-006 | Projects and membership | Implemented, integration-tested | Project membership scoping and resource privacy |
| TF-007 | Tasks and assignment rules | Implemented, integration-tested | Assigned member status transition and task-management authorization |
| TF-008 | Comments | Implemented, integration-tested | Project-member comment creation/read, author attribution, non-member isolation |
| TF-009 | Dashboard analytics | Implemented, integration-tested | KPI counts, status breakdown, and project-scope isolation |
| TF-010 | React shell and auth screens | Implemented, CI-verified | Node.js 22, Vitest, Vite production build |
| TF-011 | Dashboard/projects/tasks/comments UI | Source implemented | Responsive UI and API integration; browser E2E remains |
| TF-012 | QA/security/review/docs | In progress | API integration coverage expanded; browser E2E and production security hardening remain |

## PostgreSQL integration coverage

The integration suite uses Testcontainers to start a real PostgreSQL 17 container. Tests cover:
- Registration always creates a Member; login accepts normalized email.
- Project lists and detail endpoints enforce membership scope.
- Assigned members may change task status but cannot edit task fields.
- Only project managers may create tasks.
- Project members can create/read comments with author attribution; non-members cannot read them.
- Dashboard KPI/status counts include only projects visible to the current user.
- A changed global role invalidates previously issued JWTs.

The database is isolated for the test class and reset between test cases.

## Remaining before production readiness

- Add browser E2E and accessibility checks.
- Complete production security hardening in `security-review.md`.
- Configure a deployment target, runtime secrets, health checks, rollback, and an approved deployment trigger before enabling CD.
