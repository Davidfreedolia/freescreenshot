@echo off
set "DOTNET_ROOT=C:\Users\David\.dotnet"
set "PATH=C:\Users\David\.dotnet;%PATH%"
cd /d "C:\Projects\freescreenshoot (1)"
echo === publish starting %DATE% %TIME% > C:\Temp\publish.log
"C:\Users\David\.dotnet\dotnet.exe" publish "C:\Projects\freescreenshoot (1)\src\FreeScreenshot.Tray\FreeScreenshot.Tray.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "C:\Projects\freescreenshoot (1)\publish\win-x64" >> C:\Temp\publish.log 2>&1
echo === publish exit=%ERRORLEVEL% >> C:\Temp\publish.log
