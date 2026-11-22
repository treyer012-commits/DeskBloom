param([Parameter(Mandatory=$true)][string]$Compiler,[string]$OutputDirectory=(Join-Path $PSScriptRoot '..\artifacts'),[switch]$TestBuild,[string]$TestDataRoot)
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$out=[IO.Path]::GetFullPath($OutputDirectory)
$payload=Join-Path $out 'payload';[void](New-Item -ItemType Directory -Path $payload -Force)
& (Join-Path $repo 'src\build-DeskBloom.ps1') -OutputName DeskBloom.exe -OutputDirectory $payload
if(-not(Test-Path -LiteralPath $Compiler)){throw 'Inno Setup ISCC.exe was not found'}
$arguments=@('/Qp',('/DPayloadDir='+$payload),('/DOutputDir='+$out))
if($TestBuild){if(-not$TestDataRoot){throw 'TestDataRoot is required'};$arguments+='/DTESTBUILD';$arguments+=('/DTestDataRoot='+[IO.Path]::GetFullPath($TestDataRoot))}
$arguments+=(Join-Path $PSScriptRoot 'DeskBloom.iss')
& $Compiler @arguments
if($LASTEXITCODE-ne0){throw 'Installer compilation failed'}
