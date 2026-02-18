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

if not exist backups mkdir backups
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set TS=%%i
docker compose --env-file "%ENV_FILE%" exec -T postgres sh -c "pg_dump -U \"$POSTGRES_USER\" -d \"$POSTGRES_DB\" -Fc" > backups\business_dashboard-%TS%.dump
echo Backup creado en backups\
