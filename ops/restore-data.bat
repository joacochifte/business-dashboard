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

if "%~1"=="" (
  echo Uso: restore-data.bat backups\archivo.dump
  exit /b 1
)
docker compose --env-file "%ENV_FILE%" exec -T postgres sh -c "pg_restore -U \"$POSTGRES_USER\" -d \"$POSTGRES_DB\" --clean --if-exists --no-owner" < "%~1"
echo Restore OK
