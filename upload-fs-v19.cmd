@echo off
set "PATH=C:\Program Files\nodejs;%PATH%"
cd /d "C:\Projects\freedolia-hub"
"C:\Program Files\nodejs\node.exe" "C:\Users\David\AppData\Local\npm-cache\_npx\32026684e21afda6\node_modules\wrangler\bin\wrangler.js" r2 object put freescreenshot/Setup_FreeScreenshot.exe --file="C:\Projects\freescreenshoot (1)\installer\dist\Setup_FreeScreenshot.exe" --content-type application/octet-stream --remote > C:\Temp\r2up.log 2>&1
echo EXIT=%ERRORLEVEL% >> C:\Temp\r2up.log
