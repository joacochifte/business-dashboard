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

docker compose --env-file "%ENV_FILE%" up -d --build
timeout /t 10 >nul
start http://localhost:3000
