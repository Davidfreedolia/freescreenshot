; FreeScreenshot — Inno Setup script v0.0.3
; Build: dotnet publish (self-contained, single-file) then ISCC this script.
;
;   "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" installer\Setup_FreeScreenshot.iss
;
; Outputs installer\dist\Setup_FreeScreenshot.exe

#define MyAppName        "FreeScreenshot"
#define MyAppVersion     "1.8.0"
#define MyAppPublisher   "Freedolia"
#define MyAppURL         "https://freedolia.com"
#define MyAppExeName     "FreeScreenshot.exe"

[Setup]
AppId={{6E2B1D2A-3E9F-4A1F-B7C8-FREEDOLIAFSCR}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/freescreenshot
AppUpdatesURL={#MyAppURL}/freescreenshot
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright=© 2026 Freedolia. Licensed under GPLv3.

DefaultDirName={localappdata}\Programs\{#MyAppName}
UsePreviousAppDir=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

OutputDir=dist
OutputBaseFilename=Setup_FreeScreenshot
Compression=lzma2/ultra
SolidCompression=yes
LZMAUseSeparateProcess=yes

WizardStyle=modern
WizardSizePercent=110
SetupIconFile=..\brand\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
LicenseFile=..\LICENSE
ShowLanguageDialog=auto
CloseApplications=force
CloseApplicationsFilter=*.exe
RestartApplications=no

; The "How it works" wizard page (per-language).
InfoBeforeFile=info-before-ca.txt

[Languages]
Name: "ca"; MessagesFile: "compiler:Languages\Catalan.isl"; InfoBeforeFile: "info-before-ca.txt"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"; InfoBeforeFile: "info-before-es.txt"
Name: "en"; MessagesFile: "compiler:Default.isl";          InfoBeforeFile: "info-before-en.txt"

[CustomMessages]
ca.TaskAutostart=Iniciar FreeScreenshot amb Windows
ca.TaskLaunch=Llançar FreeScreenshot ara
ca.ShortcutSettings=FreeScreenshot — Configuració
es.TaskAutostart=Iniciar FreeScreenshot con Windows
es.TaskLaunch=Iniciar FreeScreenshot ahora
es.ShortcutSettings=FreeScreenshot — Configuración
en.TaskAutostart=Start FreeScreenshot with Windows
en.TaskLaunch=Launch FreeScreenshot now
en.ShortcutSettings=FreeScreenshot — Settings

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart";   Description: "{cm:TaskAutostart}";    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\FreeScreenshot.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PRIVADESA.md";                       DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE";                            DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md";                          DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}";          Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ShortcutSettings}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{userprograms}\{#MyAppName}";   Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{userdesktop}\{#MyAppName}";    Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "FreeScreenshot"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:TaskLaunch}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Show the uninstall feedback survey BEFORE removing files. The app exits
; on its own; if the user skips, no POST is sent.
Filename: "{app}\{#MyAppExeName}"; Parameters: "--uninstall-feedback"; Flags: runhidden waituntilterminated; RunOnceId: "FsUninstallFeedback"
; Then make sure the regular instance is killed before file removal.
Filename: "{cmd}"; Parameters: "/C taskkill /F /IM {#MyAppExeName}"; Flags: runhidden
