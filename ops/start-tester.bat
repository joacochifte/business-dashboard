@echo off
cd /d %~dp0..
docker compose up -d --build
timeout /t 10 >nul
start http://localhost:3000
