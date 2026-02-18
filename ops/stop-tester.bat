@echo off
cd /d %~dp0..

set "ENV_FILE=.env.local"
if not exist "%ENV_FILE%" (
  if exist ".env.local.example" (
    set "ENV_FILE=.env.local.example"
  )
)

docker compose --env-file "%ENV_FILE%" down
