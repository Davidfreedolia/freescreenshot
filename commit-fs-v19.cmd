@echo off
set "GIT=C:\Program Files\Git\bin\git.exe"
cd /d "C:\Projects\freescreenshoot (1)"
echo === status === > C:\Temp\git.log
"%GIT%" status --short >> C:\Temp\git.log 2>&1
echo === add === >> C:\Temp\git.log
"%GIT%" add -A >> C:\Temp\git.log 2>&1
echo === commit === >> C:\Temp\git.log
"%GIT%" commit -m "fix: floating toolbar pill — replace radius 999 (flat ellipse) with proper rounded corners + breathing room" >> C:\Temp\git.log 2>&1
echo === push === >> C:\Temp\git.log
"%GIT%" push origin main >> C:\Temp\git.log 2>&1
echo === done exit=%ERRORLEVEL% >> C:\Temp\git.log
