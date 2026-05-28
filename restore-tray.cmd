@echo off
set "GIT=C:\Program Files\Git\bin\git.exe"
cd /d "C:\Projects\freescreenshoot (1)"
echo === step1: restore FreeScreenshot.Tray from HEAD === > C:\Temp\restore.log
"%GIT%" checkout HEAD -- src/FreeScreenshot.Tray >> C:\Temp\restore.log 2>&1
echo === step1 exit=%ERRORLEVEL% >> C:\Temp\restore.log
