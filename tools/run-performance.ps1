param([Parameter(Mandatory=$true)][string]$Executable,[string]$Label='baseline',[int]$Repeats=1)
$ErrorActionPreference='Stop'
$root=Join-Path $PSScriptRoot ('performance-'+$Label+'-'+(Get-Date -Format 'HHmmss'))
[void](New-Item -ItemType Directory -Path $root -Force)
$summaries=@()
foreach($scenario in @('small','large')){for($iteration=1;$iteration-le$Repeats;$iteration++){
  $folder=Join-Path $root ($scenario+'-'+$iteration)
  $process=Start-Process -FilePath $Executable -ArgumentList @('--perf-test',('"'+$folder+'"'),$scenario) -PassThru -WindowStyle Hidden
  foreach($phase in @('closed','refreshed')){
    $ready=Join-Path $folder ($phase+'.ready');$limit=(Get-Date).AddSeconds(20)
    while(-not(Test-Path -LiteralPath $ready)-and -not$process.HasExited -and (Get-Date)-lt$limit){Start-Sleep -Milliseconds 50}
    if(-not(Test-Path -LiteralPath $ready)){if(Test-Path -LiteralPath (Join-Path $folder 'error.txt')){Get-Content -LiteralPath (Join-Path $folder 'error.txt')};throw ('Benchmark did not reach '+$phase)}
    $sample=Get-CimInstance Win32_PerfFormattedData_PerfProc_Process -Filter ('IDProcess='+$process.Id) | Select-Object IDProcess,WorkingSetPrivate,WorkingSet,PrivateBytes,HandleCount,ThreadCount
    $sample | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder ($phase+'-memory.json')) -Encoding utf8
    'continue' | Set-Content -LiteralPath (Join-Path $folder ($phase+'.continue'))
  }
  if(-not$process.WaitForExit(15000)-or$process.ExitCode-ne0){throw 'Benchmark failed'}
  $result=Get-Content -LiteralPath (Join-Path $folder 'result.json') -Raw | ConvertFrom-Json
  $closed=Get-Content -LiteralPath (Join-Path $folder 'closed-memory.json') -Raw | ConvertFrom-Json
  $refreshed=Get-Content -LiteralPath (Join-Path $folder 'refreshed-memory.json') -Raw | ConvertFrom-Json
  $summary=[PSCustomObject]@{Label=$Label;Scenario=$scenario;Run=$iteration;ClosedPrivateMB=[math]::Round($closed.WorkingSetPrivate/1MB,2);AfterRefreshPrivateMB=[math]::Round($refreshed.WorkingSetPrivate/1MB,2);ReadyMs=[math]::Round($result.ReadyMs,2);RefreshMs=[math]::Round($result.RefreshBatchMs,2);OpenCallMs=[math]::Round($result.OpenCallMs,2);ReopenMs=[math]::Round($result.ReopenReadyMs,2);StartupIcons=$result.StartupIconLoads;StartupTiles=$result.StartupTiles;TotalIcons=$result.TotalIconLoads;TotalTiles=$result.TotalTiles;ReusedWindows=$result.ReusedWindows;IdleCpuMs=$result.IdleCpuMs}
  $summaries+=$summary;$summary | ConvertTo-Json -Compress
}}
$summaries | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $root 'summary.json') -Encoding utf8
$root | Set-Content -LiteralPath (Join-Path $PSScriptRoot ('last-performance-'+$Label+'.txt'))
