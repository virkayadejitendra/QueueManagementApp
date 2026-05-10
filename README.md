# Queue Management App

Simple queue management MVP built with ASP.NET Core Web API, SQLite, and an Angular frontend planned for the UI.

## Current Status

Day 1 foundation is implemented:

- ASP.NET Core Web API project
- SQLite `AppDbContext` setup
- Health endpoint: `GET /api/health`
- OpenAPI contract: `openapi/queue-management-api.yaml`
- First API integration test

The Angular app is not scaffolded yet because Node.js/npm are not available in the current environment.

## Prerequisites

- .NET SDK 10
- Node.js and npm, only needed when creating/running the Angular frontend

Check .NET:

```powershell
dotnet --version
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

## Frontend Setup Later

After installing Node.js and npm, create the Angular app from the repo root:

```powershell
npx @angular/cli new frontend --routing --style=scss
```

Then follow `frontend/README.txt` for the planned frontend screens.
