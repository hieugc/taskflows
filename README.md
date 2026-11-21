# TaskFlow — Task Management System

TaskFlow is a responsive project and task management application built with ASP.NET Core 10, React + TypeScript, and PostgreSQL.

## MVP capabilities

- Email/password registration and login with signed JWT access tokens.
- Global RBAC roles: `Admin`, `Manager`, and `Member`.
- Projects with project-level membership (`Manager` / `Member`).
- Tasks with assignee, priority, due date, status, and description.
- Project task comments.
- Dashboard KPIs and project/task analytics.
- Responsive React application with dashboard, projects, project detail, and comments UI.

## Architecture

A modular monolith with a React SPA and one ASP.NET Core API. The API owns all authorization decisions; hiding a button in the frontend is never treated as access control. PostgreSQL is the source of truth.

## Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker Engine / Docker Desktop (for local PostgreSQL and PostgreSQL integration tests)

## Quick start

1. Copy `.env.example` to `.env` and set a unique `JWT_SIGNING_KEY` (at least 32 bytes).
2. Start PostgreSQL: `docker compose up -d db`.
3. The schema is initialized automatically by Docker on the first empty database volume. If the volume already exists, run `docker exec -i taskflow-postgres psql -U taskflow -d taskflow < db/init/001-schema.sql`.
4. Start the API:
   ```bash
   cd backend/TaskFlow.Api
   set -a; source ../../.env; set +a
   dotnet restore
   dotnet run
   ```
5. Start the frontend:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
6. Open `http://localhost:5173`.

The API listens on `http://localhost:5080` in the supplied launch profile. The frontend proxies `/api` to the API. Run backend tests with `dotnet test TaskFlow.sln -c Release`; these include PostgreSQL integration tests and require a working Docker daemon. Run frontend tests with `cd frontend && npm test`, and build the frontend with `cd frontend && npm run build`.

## CI and browser verification

GitHub Actions runs on the repository's self-hosted macOS ARM64 runner for pushes to `main`, pull requests targeting `main`, and manual dispatch. The backend job runs .NET 10 Release build and PostgreSQL Testcontainers integration tests. The frontend job runs Vitest, the Vite production build, and Playwright Chromium E2E tests. Browser checks cover authentication redirects/errors, project creation, task creation/status changes, comments, and an axe accessibility smoke test for the sign-in form. TRX and Playwright reports are uploaded as 14-day artifacts.

## Development notes

- Registration always creates a `Member`; public registration cannot self-assign privileged roles. Login/register are limited to 10 requests per client IP per minute; Admin bootstrap is limited to 5 per 15 minutes.
- The first Admin can be provisioned once through `POST /api/setup/bootstrap-admin` when `BOOTSTRAP_ADMIN_TOKEN` is configured (minimum 16 characters). Follow `docs/first-admin.md`, then remove the secret from the environment. Admins can manage global roles through `/api/admin/users/{id}/role`. Role changes invalidate existing access tokens; users must sign in again.
- API responses include security headers (`nosniff`, frame denial, referrer/permissions policy, restrictive CSP, and HSTS on HTTPS). This MVP uses short-lived bearer tokens stored in `sessionStorage` in the browser. For production, move to a refresh-token flow using Secure, HttpOnly, SameSite cookies, add CSRF protection where applicable, and rotate/revoke refresh tokens.
- `db/init/001-schema.sql` is the initial local bootstrap schema. For ongoing production schema evolution, generate and review EF Core migrations; do not rely on `EnsureCreated`.
- No production deployment, DNS change, or paid service has been performed.

## Verification status

The self-hosted CI workflow is the source of truth for backend restore/build/unit/integration tests, frontend unit tests/build, and browser E2E checks. PostgreSQL integration tests start an isolated `postgres:17-alpine` container. Production deployment is not configured until its target and runtime environment are defined.
