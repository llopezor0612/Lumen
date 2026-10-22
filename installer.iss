#define MyAppName "Lumen"
#define MyAppVersion "1.4.0"
#define MyAppPublisher "Lumen"
#define MyAppURL "http://localhost:8765"
#define MyAppExeName "Lumen-Installed.bat"

[Setup]
AppId={{B77E9AB2-77E2-4E2C-95A8-4F54A3E8C7F0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\Lumen
DefaultGroupName=Lumen
OutputDir=installer-output
OutputBaseFilename=Lumen-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\Lumen.exe
CloseApplications=yes
RestartApplications=no
WizardStyle=modern

[Files]
Source: "..\..\dist\Lumen\*"; DestDir: "{app}"; Excludes: ".env,lumen.db"; Flags: recursesubdirs ignoreversion
Source: "desktop-publish\LumenDesktop.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".env"; DestDir: "{app}"; DestName: ".env"; Flags: onlyifdoesntexist ignoreversion
Source: "Lumen-Installed.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autodesktop}\Lumen"; Filename: "{app}\Lumen-Installed.bat"; WorkingDir: "{app}"; IconFilename: "{app}\Lumen.exe"
Name: "{group}\Lumen"; Filename: "{app}\Lumen-Installed.bat"; WorkingDir: "{app}"; IconFilename: "{app}\Lumen.exe"
Name: "{group}\Desinstalar Lumen"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\Lumen-Installed.bat"; Description: "Abrir Lumen ahora"; Flags: postinstall nowait skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\.env"
