@echo off
set "GIT=C:\Program Files\Git\bin\git.exe"
cd /d "C:\Projects\freescreenshoot (1)"
echo === status === > C:\Temp\git.log
"%GIT%" status --short >> C:\Temp\git.log 2>&1
echo === add === >> C:\Temp\git.log
"%GIT%" add -A >> C:\Temp\git.log 2>&1
echo === commit === >> C:\Temp\git.log
"%GIT%" commit -m "feat: v2.1 — clean upgrade migration (kills old FreeScreenshot orphan in registry + folder), donation surfaced (tray menu + onboarding footer), correct Stripe payment link, SmartScreen pre-warning on landings, lead-capture flow documented for cowork." >> C:\Temp\git.log 2>&1
echo === push === >> C:\Temp\git.log
"%GIT%" push origin main >> C:\Temp\git.log 2>&1
echo === done exit=%ERRORLEVEL% >> C:\Temp\git.log
