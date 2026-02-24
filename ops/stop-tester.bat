@echo off
cd /d %~dp0..

set "ENV_FILE=.env.local"
if not exist "%ENV_FILE%" (
  if exist ".env.local.example" (
    set "ENV_FILE=.env.local.example"
  ) else (
    echo Missing .env.local and .env.local.example
    exit /b 1
  )
)

docker compose --env-file "%ENV_FILE%" down
if errorlevel 1 (
  echo Failed to stop containers.
  exit /b 1
)

echo Containers stopped.
