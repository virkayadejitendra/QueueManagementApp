# Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.10.

## Application configuration

Environment-specific settings live in `src/environments`.

- `src/environments/environment.ts` is used for local development.
- `src/environments/environment.prod.ts` replaces it for production builds.
- `apiBaseUrl` controls which backend API Angular calls.

For local development, the API currently points to:

```ts
apiBaseUrl: 'http://localhost:5020'
```

For production, `apiBaseUrl` is set to an empty string so API calls use the same host as the Angular app, for example `/api/owners/register`. If the production API is hosted separately, set `apiBaseUrl` in `environment.prod.ts` to that deployed API URL.

## Development server

To start a local development server, run:

```powershell
npm start
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

Use `npm start` instead of running `ng serve` directly. Angular CLI is installed locally in this project, so Windows may show this error if `ng` is not installed globally:

```text
'ng' is not recognized as an internal or external command,
operable program or batch file.
```

If you want to call Angular CLI directly without installing it globally, use:

```powershell
npx ng serve
```

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
npm run build:prod
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

Useful build commands:

```powershell
npm run build:dev
npm run build:prod
```

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
