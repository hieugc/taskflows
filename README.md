# TaskFlow — Task Management System

TaskFlow is a responsive project and task management application built with ASP.NET Core 10, React + TypeScript, and PostgreSQL.

## MVP capabilities

- Email/password registration and login with signed JWT access tokens.
- Global RBAC roles: `Admin`, `Manager`, and `Member`.
- Projects with project-level membership (`Manager` / `Member`).
- Tasks with assignee, priority, due date, status, and description.
- Project task comments.
- Dashboard KPIs and project/task analytics.
- Responsive React application with dashboard, projects, project detail, and authentication screens.

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

## CI

GitHub Actions runs on the repository's self-hosted macOS ARM64 runner for pushes to `main`, pull requests targeting `main`, and manual dispatch. The backend job verifies Docker availability, restores and builds the .NET 10 solution in Release, then runs unit tests and real-PostgreSQL integration tests using Testcontainers. The frontend job runs Vitest and the Vite production build using Node.js 22.

## Development notes

- Registration always creates a `Member`; public registration cannot self-assign privileged roles.
- The first Admin can be provisioned once through `POST /api/setup/bootstrap-admin` when `BOOTSTRAP_ADMIN_TOKEN` is configured (minimum 16 characters). Follow `docs/first-admin.md`, then remove the secret from the environment. Admins can manage global roles through `/api/admin/users/{id}/role`. Role changes invalidate existing access tokens; users must sign in again.
- This MVP uses short-lived bearer tokens stored in `sessionStorage` in the browser. For production, move to a refresh-token flow using Secure, HttpOnly, SameSite cookies, add CSRF protection where applicable, and rotate/revoke refresh tokens.
- `db/init/001-schema.sql` is the initial local bootstrap schema. For ongoing production schema evolution, generate and review EF Core migrations; do not rely on `EnsureCreated`.
- No production deployment, DNS change, or paid service has been performed.

## Verification status

The self-hosted CI workflow is the source of truth for backend restore/build/unit/integration tests and frontend test/build results. PostgreSQL integration tests start an isolated `postgres:17-alpine` container and clean database state between test cases. Production deployment is not configured until a deployment target and its runtime environment are defined.
