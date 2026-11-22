# Memory after real UI use in 4.1

The old cold-start benchmark did not exercise repeated appearance/name editing. A visible-window workload reproduced the reported growth: 4.0 reached approximately 100 MB private working set, even though its managed heap was about 8 MB. The main difference was native rendering resources, not the size of the stored files or source code.

The optimized build sets WPF `RenderOptions.ProcessRenderMode` to `SoftwareOnly` before creating windows. This is scoped to DeskBloom; no Windows-wide graphics setting is changed. It also updates appearance in place, releases collapsed file controls and closed dialog content, and uses finite animations capped at 45 fps. Colors, icons, formatted names and animations remain available.

## Visible-window workload

Same PC, x86 processes, one recorded run per version. Each has five visible desktop panels and 18 plain text files per group. The sequence opens and closes 11 appearance dialogs and 11 rename dialogs, applies colors/styles, then opens and closes all five groups ten times. No screenshots are captured during the measurements. The final idle phase waits 2.5 seconds. These fixture files contain no user data.

| Phase | 4.0 private working set | 4.1 private working set |
| --- | ---: | ---: |
| Cold, collapsed panels | 46.70 MB | 15.30 MB |
| First appearance dialog closed | 50.63 MB | 18.36 MB |
| First rename dialog closed | 52.22 MB | 18.63 MB |
| After repeated dialogs | 90.83 MB | 26.87 MB |
| After group toggles | 99.12 MB | 27.04 MB |
| After idle | 97.32 MB | 27.41 MB |

The final difference is about 72%. CPU time during all-panel toggling fell from 4375 ms to 3109.375 ms over the workload, on a 16-logical-processor machine. This measures process CPU time, not animation frame latency. Limiting animation rate reduces rendering work; software rendering can cost more CPU for much larger interfaces. Idle CPU on the installed four-group configuration was 0 ms during the short sample.

The existing four-group installation measured 12.31–12.35 MB immediately after upgrading, with group settings, positions, styles and file inventory preserved. This is a separate cold-start observation, not the like-for-like stress comparison above. Open groups, shell icon providers, images and ongoing editing can increase memory use. There is no fixed 12 MB or 27 MB cap.

Measurements use Windows `WorkingSetPrivate`, matching the reported private memory footprint. Total working set includes shared Windows/.NET pages and is larger. Private bytes measure committed private memory and are also recorded in the raw results. No forced GC, process cycling or working-set trimming is used to reduce the reported figures. `GC.GetTotalMemory(false)` only observes the managed heap.

## Reproduce

Build developer tools, which are excluded from normal release executables:

```powershell
.\src\build-DeskBloom.ps1 -Diagnostics -OutputName DeskBloom-checks.exe -OutputDirectory .\artifacts
.\tools\run-memory-probe.ps1 -Executable (Resolve-Path .\artifacts\DeskBloom-checks.exe) -Mode software -Label software
```

The runner creates isolated files and temporary visible windows; run it in a Windows desktop session. `-Mode hardware` selects the original render preference for comparison. The checked-in 4.0 baseline was compiled before the 4.1 changes, with the same workload added. The raw JSON has no personal file paths. Developer samples include managed heap size and loaded graphics-module names; module image sizes are not added up as private RAM.

The release executable shrank from 160,768 bytes to 121,856 bytes by excluding tests, benchmarks and demo generators. Source files and the `-Diagnostics` build option remain available. This reduces the shipped code, but most of the measured RAM reduction comes from rendering changes.

References: [Microsoft: ProcessRenderMode](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.renderoptions.processrendermode), [WPF hardware and software rendering costs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-taking-advantage-of-hardware).
