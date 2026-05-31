# Queue Management App Deployment Steps

This document records how the Queue Management App was deployed with:

- Angular frontend on GitHub Pages
- ASP.NET Core API on Render
- Docker-based backend deployment
- PostgreSQL for hosted persistent storage

## Current Live URLs

Frontend:

```text
https://virkayadejitendra.github.io/QueueManagementApp/
```

Backend API:

```text
https://queuemanagementapp.onrender.com
```

Health check:

```text
https://queuemanagementapp.onrender.com/api/health
```

## Backend Deployment on Render

Render does not provide a native ASP.NET Core runtime for Web Services, so the API is deployed using Docker.

Render service settings:

```text
Service Type: Web Service
Language: Docker
Branch: main
Root Directory: blank
Dockerfile Path: Dockerfile
Region: Ohio or nearest available region
```

The root-level `Dockerfile` publishes the API project from:

```text
src/QueueManagement.Api
```

The container starts the API using Render's dynamic port:

```text
dotnet QueueManagement.Api.dll --urls http://0.0.0.0:${PORT:-8080}
```

## Render Environment Variables

Set these in Render under:

```text
Service -> Environment
```

Required:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<PostgreSQL connection string>
Jwt__Issuer=QueueManagement.Api
Jwt__Audience=QueueManagement.Frontend
Jwt__SigningKey=<long-random-production-secret>
Jwt__ExpiresMinutes=60
Cors__AllowedOrigins__0=https://virkayadejitendra.github.io
```

The backend still supports the local SQLite connection from `appsettings.json`,
but Render should use a managed PostgreSQL database so data survives redeploys
and container restarts.

Important CORS note:

Use only the origin:

```text
https://virkayadejitendra.github.io
```

Do not use:

```text
https://virkayadejitendra.github.io/QueueManagementApp
https://virkayadejitendra.github.io/QueueManagementApp/login
```

Those include path segments and are not valid CORS origins.

## Frontend Deployment on GitHub Pages

The Angular app is deployed from GitHub Actions.

Production API URL is configured in:

```text
frontend/src/environments/environment.prod.ts
```

Current value:

```ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://queuemanagementapp.onrender.com'
};
```

The GitHub Actions workflow builds Angular with:

```text
npm run build:prod -- --base-href /QueueManagementApp/
```

This is required because the app is hosted under the repository path:

```text
/QueueManagementApp/
```

The workflow also copies the generated Angular entry point to `404.html`:

```text
cp dist/frontend/browser/index.html dist/frontend/browser/404.html
```

This is required for Angular client-side routes on GitHub Pages. Without it,
refreshing a route such as `/QueueManagementApp/login` makes GitHub Pages look
for a real file at that path and return its own 404 page before Angular can
handle the route.

## Deployment Flow

1. Commit code changes.

```bash
git add .
git commit -m "Describe deployment change"
git push origin main
```

2. GitHub Actions builds and deploys the Angular app to GitHub Pages.

3. Render detects the push to `main` and redeploys the backend Docker service.

4. After deploy, test the backend health endpoint:

```bash
curl https://queuemanagementapp.onrender.com/api/health
```

Expected response:

```json
{
  "status": "Healthy"
}
```

5. Open the frontend:

```text
https://virkayadejitendra.github.io/QueueManagementApp/login
```

6. In browser DevTools, verify login calls the Render API:

```text
https://queuemanagementapp.onrender.com/api/auth/login
```

If the request goes to GitHub Pages instead, the deployed frontend bundle is old or `apiBaseUrl` is wrong.

## Useful API Test Commands

Health:

```bash
curl --location 'https://queuemanagementapp.onrender.com/api/health'
```

Owner registration:

```bash
curl --location 'https://queuemanagementapp.onrender.com/api/owners/register' \
--header 'Content-Type: application/json' \
--data-raw '{
  "ownerName": "Test Owner",
  "email": "testowner1@example.com",
  "mobile": "9876543210",
  "password": "Password@123",
  "businessName": "Demo Restaurant",
  "locationName": "Main Branch",
  "address": "Demo Street, Demo City",
  "businessMobile": "9876543210"
}'
```

Login:

```bash
curl --location 'https://queuemanagementapp.onrender.com/api/auth/login' \
--header 'Content-Type: application/json' \
--data-raw '{
  "identifier": "testowner1@example.com",
  "password": "Password@123"
}'
```

## CORS Troubleshooting

If the browser blocks the API call, test the preflight request:

```bash
curl -i -X OPTIONS https://queuemanagementapp.onrender.com/api/auth/login \
  -H "Origin: https://virkayadejitendra.github.io" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: content-type"
```

Expected response should include:

```text
access-control-allow-origin: https://virkayadejitendra.github.io
```

If this header is missing:

- Confirm `Cors__AllowedOrigins__0` is set in Render.
- Redeploy the Render service.
- Use "Clear build cache & deploy" if needed.

## Database

Local development defaults to SQLite:

```text
Data Source=queue-management.db
```

Render production should use PostgreSQL through:

```text
ConnectionStrings__DefaultConnection=<PostgreSQL connection string>
```

The API accepts either standard Npgsql format:

```text
Host=<host>;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require
```

or a provider URL format:

```text
postgresql://<user>:<password>@<host>/<database>?sslmode=require
```

On Render free Web Services, the local filesystem is ephemeral. Do not use the
SQLite file for production because it can be reset or lost when:

- The service redeploys
- The container restarts
- Render replaces the instance
- The free instance spins down and starts again

## Free PostgreSQL Setup

Use a free managed PostgreSQL provider such as Neon, then copy the database
connection string into Render.

Architecture:

```text
GitHub Pages Angular
        |
        v
Render ASP.NET Core API
        |
        v
Managed PostgreSQL
```

After setting `ConnectionStrings__DefaultConnection`, redeploy the Render
service. The API creates the schema automatically on first startup when the
PostgreSQL database is empty.

## Common Issues

### Login Request Goes to GitHub Pages

Wrong:

```text
https://virkayadejitendra.github.io/api/auth/login
```

Correct:

```text
https://queuemanagementapp.onrender.com/api/auth/login
```

Fix:

- Check `frontend/src/environments/environment.prod.ts`
- Confirm GitHub Pages deployment completed
- Hard refresh browser with `Ctrl + Shift + R`

### API Returns 500 After Deploy

Check Render environment variables, especially:

```text
Jwt__SigningKey
```

The API requires a JWT signing key in production.

### Data Disappears After Deploy

This means Render is still using the local SQLite fallback.

Fix:

- Confirm `ConnectionStrings__DefaultConnection` is set in Render.
- Confirm the value starts with `Host=`, `Server=`, `postgres://`, or `postgresql://`.
- Redeploy the Render service.
