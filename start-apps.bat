@echo off
setlocal

set "ROOT=%~dp0"
set "API_PROJECT=%ROOT%src\QueueManagement.Api\QueueManagement.Api.csproj"
set "FRONTEND_DIR=%ROOT%frontend"
set "API_PORT=5020"
set "FRONTEND_PORT=4200"
set "FRONTEND_URL=http://localhost:%FRONTEND_PORT%"

echo Starting Queue Management API and Angular frontend...
echo.

if not exist "%API_PROJECT%" (
  echo API project not found:
  echo %API_PROJECT%
  pause
  exit /b 1
)

if not exist "%FRONTEND_DIR%\package.json" (
  echo Angular frontend package.json not found:
  echo %FRONTEND_DIR%\package.json
  pause
  exit /b 1
)

call :killPort %API_PORT%
call :killPort %FRONTEND_PORT%

start "Queue Management API" cmd /c "cd /d "%ROOT%" && dotnet run --project "%API_PROJECT%""
start "Queue Management Frontend" cmd /c "cd /d "%FRONTEND_DIR%" && npm start"

timeout /t 5 /nobreak >nul
start "" "%FRONTEND_URL%"

echo Started both applications.
echo API:      http://localhost:%API_PORT%
echo Angular:  %FRONTEND_URL%
echo.
echo Close the opened terminal windows to stop the applications.

endlocal
exit /b 0

:killPort
set "PORT=%~1"
echo Checking port %PORT%...

for /f "tokens=5" %%P in ('netstat -ano ^| findstr /r /c:":%PORT% .*LISTENING"') do (
  echo Stopping process %%P on port %PORT%...
  taskkill /f /pid %%P >nul 2>nul
)

exit /b 0
