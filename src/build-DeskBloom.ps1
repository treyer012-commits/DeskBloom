param([string]$OutputName = 'DeskBloom.exe',[string]$OutputDirectory = (Join-Path $PSScriptRoot '..'),[ValidateSet('x86','x64','anycpu')][string]$Platform = 'x86',[switch]$Diagnostics)
$ErrorActionPreference = 'Stop'
$base = $PSScriptRoot
$framework = if ($Platform -eq 'x86') { 'C:\Windows\Microsoft.NET\Framework\v4.0.30319' } else { 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319' }
$compiler = Join-Path $framework 'csc.exe'
$out = $OutputDirectory
[void](New-Item -ItemType Directory -Path $out -Force)
Add-Type -AssemblyName System.Drawing
$bitmap = New-Object System.Drawing.Bitmap 64,64
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::FromArgb(19,30,49))
$purple = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(191,168,246))
$graphics.FillRectangle($purple,12,14,40,12)
$graphics.FillRectangle([System.Drawing.Brushes]::White,14,33,9,9)
$graphics.FillRectangle([System.Drawing.Brushes]::White,28,33,9,9)
$graphics.FillRectangle([System.Drawing.Brushes]::White,42,33,9,9)
$nativeIcon = $bitmap.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($nativeIcon)
$stream = [System.IO.File]::Create((Join-Path $base 'DeskBloom.ico'))
$icon.Save($stream)
$stream.Dispose(); $icon.Dispose(); $purple.Dispose(); $graphics.Dispose(); $bitmap.Dispose()
$references = @('System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Runtime.Serialization.dll','WPF\WindowsBase.dll','WPF\PresentationCore.dll','WPF\PresentationFramework.dll','WPF\UIAutomationTypes.dll','System.Xaml.dll') | ForEach-Object { '/reference:' + (Join-Path $framework $_) }
$vb = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Microsoft.VisualBasic.dll'
if (-not (Test-Path -LiteralPath $vb)) { $vb = 'C:\Windows\Microsoft.NET\assembly\GAC_MSIL\Microsoft.VisualBasic\v4.0_10.0.0.0__b03f5f7f11d50a3a\Microsoft.VisualBasic.dll' }
$references += '/reference:' + $vb
$diagnosticFiles=@('Tests.cs','ToggleTests.cs','OptimizationTests.cs','AppearanceTests.cs','PerformanceHarness.cs','MemoryProbe.cs','Demo.cs')
$sources=Get-ChildItem -LiteralPath $base -Filter '*.cs' | Where-Object {$Diagnostics -or $_.Name -notin $diagnosticFiles} | ForEach-Object {$_.FullName}
$defines=if($Diagnostics){'/define:DIAGNOSTICS'}else{'/define:RELEASE'}
& $compiler /nologo /target:winexe ("/platform:" + $Platform) /optimize+ /codepage:65001 $defines ('/out:' + (Join-Path $out $OutputName)) ('/win32manifest:' + (Join-Path $base 'DeskBloom.manifest')) ('/win32icon:' + (Join-Path $base 'DeskBloom.ico')) $references $sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Get-Item -LiteralPath (Join-Path $out $OutputName) | Select-Object Name,Length
