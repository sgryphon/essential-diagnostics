powershell.exe -ExecutionPolicy Bypass -Command "%~dpn0" %*
if errorlevel 1 echo SCRIPT FAILED & exit /b %errorlevel%
