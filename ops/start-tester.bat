@echo off
cd /d %~dp0..

set "ENV_FILE=.env.local"
if not exist "%ENV_FILE%" (
  if exist ".env.local.example" (
    copy /Y ".env.local.example" "%ENV_FILE%" >nul
    echo Created %ENV_FILE% from .env.local.example
  ) else (
    echo Missing .env.local and .env.local.example
    exit /b 1
  )
)

set "FRONTEND_HOST_PORT=3000"
for /f "tokens=1,* delims==" %%A in ('findstr /B /I "FRONTEND_HOST_PORT=" "%ENV_FILE%"') do set "FRONTEND_HOST_PORT=%%B"

set "BUILD_FLAG="
if /I "%1"=="--build" set "BUILD_FLAG=--build"

docker compose --env-file "%ENV_FILE%" up -d %BUILD_FLAG%
if errorlevel 1 (
  echo Failed to start containers.
  exit /b 1
)

timeout /t 10 >nul
start "" "http://localhost:%FRONTEND_HOST_PORT%"
