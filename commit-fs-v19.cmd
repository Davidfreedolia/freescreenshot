@echo off
set "GIT=C:\Program Files\Git\bin\git.exe"
cd /d "C:\Projects\freescreenshoot (1)"
echo === status === > C:\Temp\git.log
"%GIT%" status --short >> C:\Temp\git.log 2>&1
echo === add === >> C:\Temp\git.log
"%GIT%" add -A >> C:\Temp\git.log 2>&1
echo === commit === >> C:\Temp\git.log
"%GIT%" commit -m "docs: AGENTS.md now declares the house rule — every commit must update AGENTS.md and PROJECT_LOG.md. PROJECT_LOG.md gains v2.0.1 entry." >> C:\Temp\git.log 2>&1
echo === push === >> C:\Temp\git.log
"%GIT%" push origin main >> C:\Temp\git.log 2>&1
echo === done exit=%ERRORLEVEL% >> C:\Temp\git.log
