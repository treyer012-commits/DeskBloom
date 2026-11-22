param([Parameter(Mandatory=$true)][string]$Executable,[string]$Mode='hardware',[string]$Label='baseline')
$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot ('memory-'+$Label+'-'+$Mode+'-'+(Get-Date -Format 'HHmmss'))
$process=Start-Process -FilePath $Executable -ArgumentList @('--memory-test',('"'+$root+'"'),$Mode) -PassThru -WindowStyle Hidden
$summaries=@()
foreach($phase in @('cold','appearance','rename','dialogs','toggles','idle')){
  $ready=Join-Path $root ($phase+'.ready');$limit=(Get-Date).AddSeconds(45)
  while(-not(Test-Path -LiteralPath $ready)-and-not$process.HasExited-and(Get-Date)-lt$limit){Start-Sleep -Milliseconds 50}
  if(-not(Test-Path -LiteralPath $ready)){if(Test-Path -LiteralPath (Join-Path $root 'error.txt')){Get-Content -LiteralPath (Join-Path $root 'error.txt')};throw ('Probe failed at '+$phase)}
  $counter=Get-CimInstance Win32_PerfFormattedData_PerfProc_Process -Filter ('IDProcess = '+$process.Id)
  $own=Get-Content -LiteralPath (Join-Path $root ($phase+'-internal.json')) -Raw | ConvertFrom-Json
  $summary=[PSCustomObject]@{Phase=$phase;PrivateWorkingSetMB=[math]::Round($counter.WorkingSetPrivate/1MB,2);PrivateBytesMB=[math]::Round($counter.PrivateBytes/1MB,2);ManagedMB=[math]::Round($own.ManagedBytes/1MB,2);LiveDialogs=$own.LiveDialogReferences;Mode=$own.RenderMode;GraphicsModules=$own.GraphicsModules}
  $summaries+=$summary;$summary | ConvertTo-Json -Compress
  'continue' | Set-Content -LiteralPath (Join-Path $root ($phase+'.continue'))
}
if(-not$process.WaitForExit(10000)-or$process.ExitCode-ne0){throw 'Probe process failed'}
$summaries | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $root 'summary.json') -Encoding utf8
$root | Set-Content -LiteralPath (Join-Path $PSScriptRoot ('last-memory-'+$Label+'-'+$Mode+'.txt'))
