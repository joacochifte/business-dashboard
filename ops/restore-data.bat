@echo off
cd /d %~dp0..
if "%~1"=="" (
  echo Uso: restore-data.bat backups\archivo.dump
  exit /b 1
)
docker exec -i business-dashboard-postgres pg_restore -U postgres -d business_dashboard --clean --if-exists --no-owner < "%~1"
echo Restore OK
