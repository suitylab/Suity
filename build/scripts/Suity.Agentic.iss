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
OutputDir=build
OutputBaseFilename=setup-suity-agentic-{#MyAppVersion}
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

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Code]
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;
  if not MsgBox(CustomMessage('PrerequisiteMessage'), mbConfirmation, MB_YESNO) = IDYES then
  begin
    Result := False;
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
