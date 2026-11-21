# Security Review — MVP Design

## Implemented controls in source
- Password hashing via ASP.NET Core `PasswordHasher<TUser>`.
- Registration cannot choose a global role; it always creates `Member`.
- JWT signing key is loaded from configuration/environment and validated at startup.
- Every authenticated request checks that the user still exists and the signed global-role claim matches the current database role, so role changes invalidate old access tokens.
- Role claims are included in signed tokens.
- Project access is checked server-side in project/task/comment operations.
- SQL access uses EF Core parameterized queries.
- CORS is restricted to configured origins.
- Login and registration are limited to 10 requests per client IP per minute; one-time Admin bootstrap is limited to 5 requests per client IP per 15 minutes.
- API responses include `nosniff`, frame-denial, referrer, permissions, and restrictive Content Security Policy headers; HSTS is emitted on HTTPS responses.
- Unexpected errors are returned as Problem Details without stack traces in production.

## Required before production
- Replace browser `sessionStorage` bearer token with rotating refresh-token flow using Secure, HttpOnly, SameSite cookies and appropriate CSRF protection.
- Add account lockout strategy, email verification, password reset, and credential abuse monitoring.
- The current per-request role freshness check adds a database lookup; evaluate a short-lived cache with explicit invalidation if request volume warrants it.
- Remove `BOOTSTRAP_ADMIN_TOKEN` immediately after first Admin provisioning; the endpoint is disabled when the secret is absent and refuses to run once an Admin exists. Protect the secret as a one-time operator credential.
- Add audit logging for role changes, membership changes, and sensitive project/task operations.
- Add backup/restore drills, retention rules, dependency scanning, secret scanning, and monitoring.
- Review all endpoints against an authorization matrix before external exposure.

## Residual risks
- This is an MVP scaffold and has not undergone a live penetration test.
- The single-workspace data model is not tenant-isolated. Do not use it as a multi-tenant SaaS without adding and enforcing tenant boundaries.
