#define MyAppName "Suity.Agentic"
#define MyAppVersion GetEnv('VERSION')
#define MyAppPublisher "Suity"
#define MyAppExeName "Suity.Agentic.exe"
#define MyAppURL "https://github.com/ybeapps/suity-agentic"
#define MyAppIcon "..\..\src\Suity.Agentic\Assets\suity-logo.ico"
#define SourceDir GetEnv('SOURCE_DIR')

[Setup]
AppId={{8F3B5A2C-9D1E-4F6B-8A7C-3E2D1F0B9A8C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
PrivilegesRequired=lowest
OutputDir={#GetEnv('OUTPUT_DIR')}
OutputBaseFilename=setup-suity-agentic-{#MyAppVersion}-{#GetEnv('RUNTIME')}
SetupIconFile={#MyAppIcon}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequiredOverridesAllowed=dialog

[Messages]
BeveledLabel=Suity.Agentic requires .NET 10 Runtime. Please ensure you have .NET 10 Desktop Runtime installed before continuing.

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
english.PrerequisiteMessage=This application requires Microsoft .NET 10 Desktop Runtime. Please download and install it from https://dotnet.microsoft.com/download/dotnet/10.0 before continuing.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Code]
function IsDotNetInstalled(): Boolean;
var
  Version: String;
  Key: String;
begin
  Result := False;
  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  if RegQueryStringValue(HKLM, Key, 'Version', Version) then
  begin
    if Length(Version) >= 2 then
    begin
      if Copy(Version, 1, 2) = '10' then
        Result := True;
    end;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsDotNetInstalled() then
  begin
    if MsgBox(CustomMessage('PrerequisiteMessage'), mbConfirmation, MB_YESNO) <> IDYES then
    begin
      Result := False;
    end;
  end;
end;

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
