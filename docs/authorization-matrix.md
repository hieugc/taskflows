# Authorization Matrix

| Operation | Global Admin | Global Manager (not a project manager) | Project Manager (any global role) | Project Member | Assigned Member |
|---|---:|---:|---:|---:|---:|
| Bootstrap first Admin | One-time secret only, only before an Admin exists | No | No | No | No |
| List active projects | All | Membership-scoped | Membership-scoped | Membership-scoped | Membership-scoped |
| Create project | Yes | Yes | No | No | No |
| View project | Yes | If a project member | Yes | Yes | Yes |
| Update/archive project | Yes | No | Yes | No | No |
| Add/change project members | Yes | No | Yes | No | No |
| Create task | Yes | No | Yes | No | No |
| View project tasks | Yes | If a project member | Yes | Yes | Yes |
| Edit task fields/assignment | Yes | No | Yes | No | No |
| Change task status | Yes | No | Yes | No | Yes, only own assigned task |
| Delete task | Yes | No | Yes | No | No |
| Read/add comments | Yes | If a project member | Yes | Yes | Yes |
| View dashboard | All active projects | Visible projects only | Visible projects only | Visible projects only | Visible projects only |
| Change global roles | Yes | No | No | No | No |

## Enforcement notes
- API authorization is authoritative. Frontend controls are only a usability aid.
- Non-members receive 404 for project/task resources where practical to avoid resource enumeration. Members without management rights receive 403 for management operations.
- A global `Manager` role alone does not grant access to every project. Project-level `Manager` membership is required to manage an existing project.
- Global role changes are checked against the database during JWT validation and invalidate old access tokens.
