; Freezshot — Inno Setup script v2.0
; Build: dotnet publish (self-contained, single-file) then ISCC this script.
;
;   "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" installer\Setup_Freezshot.iss
;
; Outputs installer\dist\Setup_Freezshot.exe

#define MyAppName        "Freezshot"
#define MyAppVersion     "2.0.0"
#define MyAppPublisher   "Freedolia"
#define MyAppURL         "https://freedolia.com"
#define MyAppExeName     "Freezshot.exe"

[Setup]
; New AppId for Freezshot (different from FreeScreenshot, so a clean install).
AppId={{B7E1C2A4-89D5-4A6F-92C1-FREEDOLIAFRZS}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/freezshot
AppUpdatesURL={#MyAppURL}/freezshot
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
OutputBaseFilename=Setup_Freezshot
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
ca.TaskAutostart=Iniciar Freezshot amb Windows
ca.TaskLaunch=Llançar Freezshot ara
ca.ShortcutSettings=Freezshot — Configuració
es.TaskAutostart=Iniciar Freezshot con Windows
es.TaskLaunch=Iniciar Freezshot ahora
es.ShortcutSettings=Freezshot — Configuración
en.TaskAutostart=Start Freezshot with Windows
en.TaskLaunch=Launch Freezshot now
en.ShortcutSettings=Freezshot — Settings

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart";   Description: "{cm:TaskAutostart}";    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\Freezshot.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PRIVADESA.md";                  DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE";                       DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md";                     DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}";          Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ShortcutSettings}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{userprograms}\{#MyAppName}";   Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{userdesktop}\{#MyAppName}";    Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Freezshot"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:TaskLaunch}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Show the uninstall feedback survey BEFORE removing files. The app exits
; on its own; if the user skips, no POST is sent.
Filename: "{app}\{#MyAppExeName}"; Parameters: "--uninstall-feedback"; Flags: runhidden waituntilterminated; RunOnceId: "FzUninstallFeedback"
; Then make sure the regular instance is killed before file removal.
Filename: "{cmd}"; Parameters: "/C taskkill /F /IM {#MyAppExeName}"; Flags: runhidden
