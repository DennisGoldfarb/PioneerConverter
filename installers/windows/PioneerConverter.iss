#define MyAppName "PioneerConverter"
#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif
#define MyAppExeName "PioneerConverter.exe"

[Setup]
AppId={{9B8088AB-945D-4D65-AB1A-000000000001}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=PioneerConverter-win-{#MyAppVersion}-Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\..\dist\PioneerConverter-win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs

[Tasks]
Name: "addtopath"; Description: "Add application directory to PATH"; Flags: unchecked

[Registry]
Root: HKLM; Subkey: "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}"; Flags: preservestringtype; Tasks: addtopath

[Icons]
Name: "{group}\PioneerConverter"; Filename: "{app}\{#MyAppExeName}"
