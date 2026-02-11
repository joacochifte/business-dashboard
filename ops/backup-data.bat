@echo off
cd /d %~dp0..
if not exist backups mkdir backups
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set TS=%%i
docker exec -t business-dashboard-postgres pg_dump -U postgres -d business_dashboard -Fc > backups\business_dashboard-%TS%.dump
echo Backup creado en backups\
