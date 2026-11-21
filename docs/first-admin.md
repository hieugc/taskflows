# Provision the First Admin

Public registration always creates the least-privileged `Member` role. It cannot create an Admin or Manager.

1. Generate a random one-time secret of at least 16 characters.
2. Put it in the root `.env` file as `BOOTSTRAP_ADMIN_TOKEN=...`.
3. Start the API and database.
4. Call the bootstrap endpoint:

```bash
curl -X POST http://localhost:5080/api/setup/bootstrap-admin \
  -H 'Content-Type: application/json' \
  -d '{
    "bootstrapToken": "YOUR_ONE_TIME_SECRET",
    "name": "Workspace Admin",
    "email": "admin@example.com",
    "password": "Use-A-Unique-Password-Here"
  }'
```

5. Save the returned access token securely and sign in through the frontend using the same email/password.
6. Remove `BOOTSTRAP_ADMIN_TOKEN` from `.env` and restart the API. The endpoint is also guarded against running after an Admin exists.
7. Use `GET /api/admin/users` and `PATCH /api/admin/users/{id}/role` to manage global roles. At least one Admin must remain.

Do not commit `.env` or the bootstrap secret.
