# ADR-0001: Use a Modular Monolith for the MVP

- Status: Accepted
- Date: 2026-11-21

## Context
The requested product has a single API, one relational database, and a bounded set of related workflows: identity, projects, tasks, comments, and analytics. No independent deployment, high-scale service boundary, or separate team ownership requirement was provided.

## Decision
Implement one ASP.NET Core API with clear modules and PostgreSQL persistence. Keep React as a separately built SPA. Preserve domain/service boundaries so modules can be extracted later if real operational pressure justifies it.

## Consequences
- Fewer deployment units and simpler local development.
- Straightforward relational transactions and authorization checks.
- API modules share one process and database schema; avoid cross-module coupling through controller-to-controller calls.
- Microservice extraction remains a future decision based on measured scale and team boundaries, not a default.
