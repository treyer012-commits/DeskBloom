# Installer

Build on Windows with .NET Framework 4.8 and Inno Setup 6.7.4 or later:

```powershell
.\installer\build-installer.ps1 -Compiler 'C:\Path\To\Inno Setup 6\ISCC.exe' -OutputDirectory .\artifacts
```

The installer is per-user, under LocalAppData\Programs\DeskBloom. It creates a Start menu shortcut, optional desktop/startup entries, file associations and Explorer menu commands. No administrator rights are required. It checks for .NET Framework 4.8 and closes a running instance gracefully; it stops if file operations are busy. Uninstall keeps Documents group folders and settings and removes only shell entries still pointing at this installation.

For isolated install/uninstall testing, add `-TestBuild -TestDataRoot <absolute-test-path>`. This uses a separate AppId, separate registry namespace and separate application data root. It does not register the real .groups extension or stop the production instance.

The test build was installed and uninstalled successfully, including a running fixture instance. The installed executable hash, Apps entry and file association were verified; group contents and settings remained after uninstall. The normal release binary excludes tests and demo generators.
