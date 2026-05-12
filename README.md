# Queue Management App

Simple queue management MVP built with ASP.NET Core Web API, SQLite, and Angular.

## Current Status

Day 1 foundation is implemented:

- ASP.NET Core Web API project
- SQLite `AppDbContext` setup
- Health endpoint: `GET /api/health`
- OpenAPI contract: `openapi/queue-management-api.yaml`
- First API integration test
- Angular frontend project under `frontend`

## Prerequisites

- .NET SDK 10
- Node.js and npm for the Angular frontend

Check .NET:

```powershell
dotnet --version
```

Check Node.js and npm:

```powershell
node --version
npm --version
```

If PowerShell cannot find `node`/`npm` after installing Node.js, restart the terminal. If it still fails, use the full Node install path for this session:

```powershell
$env:Path = 'C:\Program Files\nodejs;' + $env:Path
```

If PowerShell blocks `npm.ps1`, use `npm.cmd` instead:

```powershell
& 'C:\Program Files\nodejs\npm.cmd' --version
```

## Restore Packages

From the repo root:

```powershell
dotnet restore QueueManagementApp.slnx
```

## Build

```powershell
dotnet build QueueManagementApp.slnx
```

## Run Tests

```powershell
dotnet test QueueManagementApp.slnx
```

To run tests without rebuilding after a successful build:

```powershell
dotnet test QueueManagementApp.slnx --no-build
```

## Run the API

From the repo root:

```powershell
dotnet run --project src\QueueManagement.Api\QueueManagement.Api.csproj
```

The API will print the local URL in the terminal, for example:

```text
http://localhost:5020
```

Test the health endpoint:

```powershell
Invoke-RestMethod http://localhost:5020/api/health
```

Expected response:

```json
{
  "status": "Healthy"
}
```

## OpenAPI Contract

The API-first contract is stored here:

```text
openapi/queue-management-api.yaml
```

For each feature:

1. Update the OpenAPI spec first.
2. Write or update API tests.
3. Implement the smallest code needed to pass the tests.
4. Run the API from the repo root.
5. Run the Angular frontend from `frontend` using `npm start`.

## Run the Frontend

From the repo root:

```powershell
cd frontend
npm start
```

Use `npm start` instead of running `ng serve` directly. This project has Angular CLI installed locally inside `frontend\node_modules`, so Windows may show this error if you run `ng serve` from the terminal:

```text
'ng' is not recognized as an internal or external command,
operable program or batch file.
```

`npm start` automatically uses the local Angular CLI for this project.

If PowerShell blocks `npm`, run:

```powershell
cd frontend
& 'C:\Program Files\nodejs\npm.cmd' start
```

Angular will print the local frontend URL in the terminal, usually:

```text
http://localhost:4200
```

## Build the Frontend

```powershell
cd frontend
npm run build
```

If PowerShell blocks `npm`, run:

```powershell
cd frontend
& 'C:\Program Files\nodejs\npm.cmd' run build
```
