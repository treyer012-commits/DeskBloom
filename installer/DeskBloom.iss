#ifndef PayloadDir
  #define PayloadDir SourcePath + "..\artifacts\payload"
#endif
#ifndef OutputDir
  #define OutputDir SourcePath + "..\artifacts"
#endif
#define AppVersion "4.1.2"
#ifdef TESTBUILD
  #define ProductId "DeskBloom.SetupTest"
  #define ProductName "DeskBloom Setup Test"
  #define ClassesRoot "Software\DeskBloomSetupTest\Classes"
  #define StartupRoot "Software\DeskBloomSetupTest\Run"
  #define DataArgs "--data """ + TestDataRoot + """ --quiet-test"
  #define ShutdownArgs "--wait-exit --data """ + TestDataRoot + """"
  #define OutputName "DeskBloom-Setup-Test"
#else
  #define ProductId "DeskBloom.treyer012"
  #define ProductName "DeskBloom"
  #define ClassesRoot "Software\Classes"
  #define StartupRoot "Software\Microsoft\Windows\CurrentVersion\Run"
  #define DataArgs ""
  #define ShutdownArgs "--wait-exit"
  #define OutputName "DeskBloom-" + AppVersion + "-Setup"
#endif

#define EntryDataArgs StringChange(DataArgs, '"', '""')

[Setup]
AppId={#ProductId}
AppName={#ProductName}
AppVersion={#AppVersion}
AppPublisher=treyer012-commits
AppPublisherURL=https://github.com/treyer012-commits/DeskBloom
AppSupportURL=https://github.com/treyer012-commits/DeskBloom/issues
AppUpdatesURL=https://github.com/treyer012-commits/DeskBloom/releases
DefaultDirName={localappdata}\Programs\{#ProductName}
DefaultGroupName={#ProductName}
PrivilegesRequired=lowest
MinVersion=10.0.17763
OutputDir={#OutputDir}
OutputBaseFilename={#OutputName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\DeskBloom.ico
UninstallDisplayIcon={app}\DeskBloom.exe
LicenseFile=..\LICENSE
DisableProgramGroupPage=yes
CloseApplications=no
RestartApplications=no
ChangesAssociations=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[CustomMessages]
english.DesktopIcon=Create a desktop shortcut
english.AutoStart=Start DeskBloom with Windows
english.AddFiles=Add to a DeskBloom group...
english.NewGroup=Create a DeskBloom group
english.NewGroupHere=Create a DeskBloom group in this folder
english.GroupType=DeskBloom group
english.RunNow=Run DeskBloom
english.Busy=DeskBloom is busy moving files. Wait for it to finish, then try again.
english.DotNet=DeskBloom needs Microsoft .NET Framework 4.8 or later. Install it and run Setup again.
russian.DesktopIcon=Создать ярлык на рабочем столе
russian.AutoStart=Запускать DeskBloom вместе с Windows
russian.AddFiles=Добавить в группу DeskBloom...
russian.NewGroup=Создать группу DeskBloom
russian.NewGroupHere=Создать группу DeskBloom в этой папке
russian.GroupType=Группа DeskBloom
russian.RunNow=Запустить DeskBloom
russian.Busy=DeskBloom занят переносом файлов. Дождитесь окончания и повторите попытку.
russian.DotNet=Для DeskBloom необходим Microsoft .NET Framework 4.8 или новее. Установите его и запустите установщик снова.

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIcon}"; Flags: unchecked
Name: "autostart"; Description: "{cm:AutoStart}"; Flags: unchecked

[Files]
Source: "{#PayloadDir}\DeskBloom.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\DeskBloom.exe"; DestName: "DeskBloomShutdown.exe"; Flags: dontcopy
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\usage-en.txt"; DestDir: "{app}"; DestName: "Read me.txt"; Flags: ignoreversion
Source: "..\docs\usage-ru.txt"; DestDir: "{app}"; DestName: "Инструкция.txt"; Flags: ignoreversion

[Icons]
Name: "{group}\DeskBloom"; Filename: "{app}\DeskBloom.exe"; Parameters: "{#EntryDataArgs}"
Name: "{autodesktop}\{#ProductName}"; Filename: "{app}\DeskBloom.exe"; Parameters: "{#EntryDataArgs}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "{#ClassesRoot}\.groups"; ValueType: string; ValueData: "DeskBloom.Group"
Root: HKCU; Subkey: "{#ClassesRoot}\DeskBloom.Group"; ValueType: string; ValueData: "{cm:GroupType}"
Root: HKCU; Subkey: "{#ClassesRoot}\DeskBloom.Group\DefaultIcon"; ValueType: string; ValueData: """{app}\DeskBloom.exe"",0"
Root: HKCU; Subkey: "{#ClassesRoot}\DeskBloom.Group\shell\open\command"; ValueType: string; ValueData: """{app}\DeskBloom.exe"" {#EntryDataArgs} --open-layout ""%1"""
Root: HKCU; Subkey: "{#ClassesRoot}\*\shell\DeskBloom.Add"; ValueType: string; ValueData: "{cm:AddFiles}"
Root: HKCU; Subkey: "{#ClassesRoot}\*\shell\DeskBloom.Add"; ValueName: "Icon"; ValueType: string; ValueData: """{app}\DeskBloom.exe"",0"
Root: HKCU; Subkey: "{#ClassesRoot}\*\shell\DeskBloom.Add"; ValueName: "MultiSelectModel"; ValueType: string; ValueData: "Single"
Root: HKCU; Subkey: "{#ClassesRoot}\*\shell\DeskBloom.Add\command"; ValueType: string; ValueData: """{app}\DeskBloom.exe"" {#EntryDataArgs} --add ""%1"""
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\shell\DeskBloom.Add"; ValueType: string; ValueData: "{cm:AddFiles}"
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\shell\DeskBloom.Add"; ValueName: "Icon"; ValueType: string; ValueData: """{app}\DeskBloom.exe"",0"
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\shell\DeskBloom.Add"; ValueName: "MultiSelectModel"; ValueType: string; ValueData: "Single"
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\shell\DeskBloom.Add\command"; ValueType: string; ValueData: """{app}\DeskBloom.exe"" {#EntryDataArgs} --add ""%1"""
Root: HKCU; Subkey: "{#ClassesRoot}\DesktopBackground\shell\DeskBloom.NewGroup"; ValueType: string; ValueData: "{cm:NewGroup}"
Root: HKCU; Subkey: "{#ClassesRoot}\DesktopBackground\shell\DeskBloom.NewGroup"; ValueName: "Icon"; ValueType: string; ValueData: """{app}\DeskBloom.exe"",0"
Root: HKCU; Subkey: "{#ClassesRoot}\DesktopBackground\shell\DeskBloom.NewGroup\command"; ValueType: string; ValueData: """{app}\DeskBloom.exe"" {#EntryDataArgs} --new-group"
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\Background\shell\DeskBloom.NewGroup"; ValueType: string; ValueData: "{cm:NewGroupHere}"
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\Background\shell\DeskBloom.NewGroup"; ValueName: "Icon"; ValueType: string; ValueData: """{app}\DeskBloom.exe"",0"
Root: HKCU; Subkey: "{#ClassesRoot}\Directory\Background\shell\DeskBloom.NewGroup\command"; ValueType: string; ValueData: """{app}\DeskBloom.exe"" {#EntryDataArgs} --new-group-in ""%V"""
Root: HKCU; Subkey: "{#StartupRoot}"; ValueName: "DeskBloom"; ValueType: string; ValueData: """{app}\DeskBloom.exe"" {#EntryDataArgs}"; Tasks: autostart

[Run]
Filename: "{app}\DeskBloom.exe"; Parameters: "{#EntryDataArgs}"; Description: "{cm:RunNow}"; Flags: nowait postinstall skipifsilent

[Code]
procedure NotifyShell(EventId, Flags: Integer; Item1, Item2: LongWord);
  external 'SHChangeNotify@shell32.dll stdcall';

function InitializeSetup: Boolean;
var Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040);
  if not Result then MsgBox(CustomMessage('DotNet'), mbError, MB_OK);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  Result := '';
  ExtractTemporaryFile('DeskBloomShutdown.exe');
  if not Exec(ExpandConstant('{tmp}\DeskBloomShutdown.exe'), '{#ShutdownArgs}', '', SW_HIDE, ewWaitUntilTerminated, Code) then
    Result := CustomMessage('Busy')
  else if Code <> 0 then Result := CustomMessage('Busy');
end;

function IsOurs(Value: String): Boolean;
begin
  Result := Pos(Lowercase(ExpandConstant('{app}\DeskBloom.exe')), Lowercase(Value)) > 0;
end;

procedure RemoveOwnedCommand(Key: String);
var Command: String;
begin
  if RegQueryStringValue(HKCU, Key + '\command', '', Command) and IsOurs(Command) then
    RegDeleteKeyIncludingSubkeys(HKCU, Key);
end;

function InitializeUninstall: Boolean;
var Code: Integer;
begin
  Result := True;
  if FileExists(ExpandConstant('{app}\DeskBloom.exe')) then begin
    Result := Exec(ExpandConstant('{app}\DeskBloom.exe'), '{#ShutdownArgs}', '', SW_HIDE, ewWaitUntilTerminated, Code);
    if Result then Result := Code = 0;
    if not Result then MsgBox(CustomMessage('Busy'), mbError, MB_OK);
  end;
end;

procedure CurUninstallStepChanged(Step: TUninstallStep);
var Command, Association: String;
begin
  if Step <> usUninstall then Exit;
  RemoveOwnedCommand('{#ClassesRoot}\*\shell\DeskBloom.Add');
  RemoveOwnedCommand('{#ClassesRoot}\Directory\shell\DeskBloom.Add');
  RemoveOwnedCommand('{#ClassesRoot}\DesktopBackground\shell\DeskBloom.NewGroup');
  RemoveOwnedCommand('{#ClassesRoot}\Directory\Background\shell\DeskBloom.NewGroup');
  if RegQueryStringValue(HKCU, '{#ClassesRoot}\DeskBloom.Group\shell\open\command', '', Command) and IsOurs(Command) then begin
    RegDeleteKeyIncludingSubkeys(HKCU, '{#ClassesRoot}\DeskBloom.Group');
    if RegQueryStringValue(HKCU, '{#ClassesRoot}\.groups', '', Association) and (Association = 'DeskBloom.Group') then
      RegDeleteValue(HKCU, '{#ClassesRoot}\.groups', '');
  end;
  if RegQueryStringValue(HKCU, '{#StartupRoot}', 'DeskBloom', Command) and IsOurs(Command) then
    RegDeleteValue(HKCU, '{#StartupRoot}', 'DeskBloom');
  NotifyShell($08000000, 0, 0, 0);
end;
