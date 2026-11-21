# Implementation Status

| ID | Package | Status | Evidence / limitation |
|---|---|---|---|
| TF-001 | Product brief and requirements | Implemented | `product-brief.md`, `requirements.md` |
| TF-002 | Architecture and data/API model | Implemented | `architecture.md`, `api-contract.md`, `authorization-matrix.md` |
| TF-003 | PostgreSQL schema and Docker Compose | Implemented | `db/init/001-schema.sql`, `docker-compose.yml`; schema exercised by PostgreSQL Testcontainers integration suite |
| TF-004 | .NET API foundation | Implemented, CI-verified | Self-hosted .NET 10 Release build |
| TF-005 | Authentication and global RBAC | Implemented, integration-tested | Registration role restriction, login, token role freshness, IP-based auth/bootstrap throttling |
| TF-006 | Projects and membership | Implemented, integration-tested | Project membership scoping and resource privacy |
| TF-007 | Tasks and assignment rules | Implemented, integration-tested | Assigned member status transition and task-management authorization |
| TF-008 | Comments | Implemented, integration-tested | Project-member comment creation/read, author attribution, non-member isolation |
| TF-009 | Dashboard analytics | Implemented, integration-tested | KPI counts, status breakdown, and project-scope isolation |
| TF-010 | React shell and auth screens | Implemented, CI-verified | Node.js 22, Vitest, Vite production build |
| TF-011 | Dashboard/projects/tasks/comments UI | Implemented, browser-tested | Playwright covers manager project/task/comment flow and task status updates |
| TF-012 | QA/security/review/docs | In progress | PostgreSQL integration tests, API security headers, IP-based throttling, and Playwright accessibility smoke checks; refresh-token flow and full accessibility audit remain |

## Automated browser coverage

Playwright runs against a real Chromium browser and a Vite development server. API requests are isolated with deterministic route fixtures so browser tests do not require a live API or persistent database. Coverage includes:
- Unauthenticated users are redirected to sign in.
- Invalid credentials are announced and do not create a session.
- A manager can create a project, add a task, update its status, and post a comment.
- The sign-in form passes axe checks for WCAG 2 A/AA and 2.1 A/AA, including automated color contrast. Keyboard navigation and a 375px mobile viewport are also covered.

## Remaining before production readiness

- Extend the current accessibility audit to registration, dashboard, project, and task screens.
- Replace sessionStorage bearer tokens with a refresh-token cookie flow and CSRF protection.
- Complete production security hardening in `security-review.md`.
- Configure a deployment target, runtime secrets, health checks, rollback, and an approved deployment trigger before enabling CD.
