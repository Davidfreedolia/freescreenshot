@echo off
set "GIT=C:\Program Files\Git\bin\git.exe"
cd /d "C:\Projects\freescreenshoot (1)"
echo === status === > C:\Temp\git.log
"%GIT%" status --short >> C:\Temp\git.log 2>&1
echo === add === >> C:\Temp\git.log
"%GIT%" add -A >> C:\Temp\git.log 2>&1
echo === commit === >> C:\Temp\git.log
"%GIT%" commit -m "docs: web/COWORK_BRIEF.md — 7-step action checklist for freedolia-hub cowork. banner-home.html news tag bumped to v2.1." >> C:\Temp\git.log 2>&1
echo === push === >> C:\Temp\git.log
"%GIT%" push origin main >> C:\Temp\git.log 2>&1
echo === done exit=%ERRORLEVEL% >> C:\Temp\git.log
