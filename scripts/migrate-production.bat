@echo off
setlocal

set ACTION=%1
if "%ACTION%"=="" set ACTION=update

set MIGRATION=%2
if "%MIGRATION%"=="" set MIGRATION=InitialCreate

set DB_CONNECTION_PARAM=%3
if not "%DB_CONNECTION_PARAM%"=="" (
	set DB_CONNECTION=%DB_CONNECTION_PARAM%
)

set REDIS_CONNECTION_PARAM=%4
if not "%REDIS_CONNECTION_PARAM%"=="" (
	set REDIS_CONNECTION=%REDIS_CONNECTION_PARAM%
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0migrate.ps1" -Action %ACTION% -Migration %MIGRATION% -Environment Production -DatabaseConnection "%DB_CONNECTION%" -RedisConnection "%REDIS_CONNECTION%"

endlocal
