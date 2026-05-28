@echo off
set "GIT=C:\Program Files\Git\bin\git.exe"
cd /d "C:\Projects\freescreenshoot (1)"
"%GIT%" checkout HEAD -- src/FreeScreenshot.Core src/FreeScreenshot.UI src/FreeScreenshot.Tray > C:\Temp\restore.log 2>&1
echo === exit=%ERRORLEVEL% >> C:\Temp\restore.log
