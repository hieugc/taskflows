# TaskFlow Implementation Plan

## Work packages

| ID | Work package | Depends on | Acceptance evidence |
|---|---|---|---|
| TF-001 | Product brief and requirements | — | Product scope and FR/AC IDs documented |
| TF-002 | Modular monolith architecture and data model | TF-001 | Architecture, schema, API contracts documented |
| TF-003 | PostgreSQL bootstrap schema and Docker Compose | TF-002 | Schema has FK/unique constraints; DB healthcheck |
| TF-004 | .NET API foundation and error handling | TF-002 | API project builds; Problem Details configured |
| TF-005 | Authentication and global RBAC | TF-004 | Register/login/me tests; least-privilege registration |
| TF-006 | Project lifecycle and project membership | TF-005 | Membership and manager authorization tests |
| TF-007 | Task lifecycle and assignment rules | TF-006 | Status/assignment authorization tests |
| TF-008 | Task comments | TF-007 | Membership-scoped comment tests |
| TF-009 | Dashboard analytics | TF-007 | Counts match visible project scope |
| TF-010 | React shell and authentication screens | TF-004 | Type-check and production build |
| TF-011 | Dashboard, projects, tasks, comments UI | TF-005..TF-009 | Responsive user flows and API integration |
| TF-012 | QA, security review, code review, docs | TF-003..TF-011 | Test evidence and known-gap report |

## Execution policy
- Use test-first implementation for behavior changes.
- Maximum three evidence-based repair attempts after an initial failure.
- Continue independent work when one package is blocked.
- No production deployment or public repository creation without explicit approval.
