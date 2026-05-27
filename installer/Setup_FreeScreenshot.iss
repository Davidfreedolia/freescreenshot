; FreeScreenshot — Inno Setup script
; Builds Setup_FreeScreenshot.exe from publish\win-x64\FreeScreenshot.exe.
;
; Requires:
;   1) dotnet publish src/FreeScreenshot.Tray/FreeScreenshot.Tray.csproj
;      --configuration Release --runtime win-x64 --self-contained true
;      -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
;      --output publish\win-x64
;   2) brand\icon.ico present (generated from PowerShell).
;
; Compile:
;   "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" installer\Setup_FreeScreenshot.iss
;
; Outputs:
;   installer\dist\Setup_FreeScreenshot.exe

#define MyAppName        "FreeScreenshot"
#define MyAppVersion     "0.0.1"
#define MyAppPublisher   "Freedolia"
#define MyAppURL         "https://freedolia.com"
#define MyAppExeName     "FreeScreenshot.exe"

[Setup]
; Stable App ID — never change between releases.
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

; Per-user install — no admin / no UAC.
DefaultDirName={localappdata}\Programs\{#MyAppName}
UsePreviousAppDir=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Output
OutputDir=dist
OutputBaseFilename=Setup_FreeScreenshot
Compression=lzma2/ultra
SolidCompression=yes
LZMAUseSeparateProcess=yes

; UX
WizardStyle=modern
WizardSizePercent=110
WizardImageAlphaFormat=defined
SetupIconFile=..\brand\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
LicenseFile=..\LICENSE
ShowLanguageDialog=auto
CloseApplications=force
CloseApplicationsFilter=*.exe
RestartApplications=no

[Languages]
Name: "ca"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
; Catalan
ca.TaskAutostart=Iniciar FreeScreenshot amb Windows
ca.TaskLaunch=Llançar FreeScreenshot ara
ca.ShortcutSettings=FreeScreenshot — Configuració
ca.UninstallSurveyTitle=Per què desinstal·les FreeScreenshot?
ca.UninstallSurveyHint=Opcional. Ens ajuda a millorar l'eina.
; Spanish
es.TaskAutostart=Iniciar FreeScreenshot con Windows
es.TaskLaunch=Iniciar FreeScreenshot ahora
es.ShortcutSettings=FreeScreenshot — Configuración
es.UninstallSurveyTitle=¿Por qué desinstalas FreeScreenshot?
es.UninstallSurveyHint=Opcional. Nos ayuda a mejorar la herramienta.
; English
en.TaskAutostart=Start FreeScreenshot with Windows
en.TaskLaunch=Launch FreeScreenshot now
en.ShortcutSettings=FreeScreenshot — Settings
en.UninstallSurveyTitle=Why are you uninstalling FreeScreenshot?
en.UninstallSurveyHint=Optional. Helps us improve the tool.

[Tasks]
Name: "autostart"; Description: "{cm:TaskAutostart}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\FreeScreenshot.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PRIVADESA.md";                       DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE";                            DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md";                          DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}";                   Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ShortcutSettings}";          Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{userprograms}\{#MyAppName}";            Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"

[Registry]
; Autostart: HKCU\...\Run entry, removed on uninstall.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "FreeScreenshot"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:TaskLaunch}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Make sure the app isn't running before files are removed.
Filename: "{cmd}"; Parameters: "/C taskkill /F /IM {#MyAppExeName}"; Flags: runhidden
