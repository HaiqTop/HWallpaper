; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "HWallpaper"
#define MyAppVersion "1.0.0.0"
#define MyAppPublisher "My Company, Inc."
#define MyAppURL "haiq.top"
#define MyAppExeName "HWallpaper.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{1491F91F-4601-4622-983A-0F807E61B219}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=D:\Git\HWallpaper
OutputBaseFilename=HWallpaper
SetupIconFile=D:\Git\HWallpaper\HWallpaper\Image\logo.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "chinese"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]     
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\HWallpaper.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\HWallpaper.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\HandyControl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\HWallpaper.Business.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\HWallpaper.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\HWallpaper.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion 
Source: "D:\Git\HWallpaper\HWallpaper\bin\Debug\Microsoft.Windows.Shell.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

