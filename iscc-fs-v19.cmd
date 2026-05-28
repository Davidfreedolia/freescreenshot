@echo off
set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
if not exist "%ISCC%" set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
echo === iscc starting %DATE% %TIME% > C:\Temp\iscc.log
"%ISCC%" "C:\Projects\freescreenshoot (1)\installer\Setup_FreeScreenshot.iss" >> C:\Temp\iscc.log 2>&1
echo === iscc exit=%ERRORLEVEL% >> C:\Temp\iscc.log
