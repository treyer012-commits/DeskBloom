# Performance in 3.2

For the current release, see [memory after repeated UI use in 4.1](memory-4.1.md). The measurements below describe earlier releases.

Compared on the same Windows PC: DeskBloom 3.1 (64-bit) and the default DeskBloom 3.2 build (32-bit). Each scenario was run twice in a separate process; values are the averages.

| Scenario | 3.1 private working set | 3.2 private working set | 3.1 refresh batch | 3.2 refresh batch |
| --- | ---: | ---: | ---: | ---: |
| 4 collapsed groups, 12 files each | 50.86 MB | 39.49 MB | 606.36 ms | 32.12 ms |
| 4 collapsed groups, 120 files each | 69.44 MB | 44.26 MB | 4146.70 ms | 36.48 ms |

After the 480-file refresh workload, private working set averaged 130.56 MB in 3.1 and 58.24 MB in 3.2. The baseline made 2400 shell icon loads; the optimized version made 1. Identical file-type icons are shared, and collapsed groups create no file tiles.

Panel preparation for that scenario took 1503.06 ms in 3.1 and 350.54 ms in 3.2. This starts at `MainWindow.Show` and ends when group metadata and layout are ready. It excludes process entry, fixture creation, settings loading and shell registration.

## Changes

- Collapsed groups keep lightweight metadata rather than hidden file controls.
- The viewport creates only visible rows and recycles rows while scrolling.
- Shell icons and thumbnails load on a background STA thread.
- The cache is bounded to 128 entries; thumbnails are bounded to 64 pixels on the longer side.
- File changes refresh only the affected group; unchanged windows and watchers are reused.
- Frequent UI saves are combined, with an immediate flush on exit.
- The default build targets x86. Use `-Platform x64` for a native 64-bit build.

## Method and reproduction

The benchmark creates isolated groups containing plain text files. Test panels are transparent to avoid disturbing the desktop; both versions use this setting. It samples Windows' `WorkingSetPrivate` counter, opens and closes one group twice, then requests three unchanged full-view refreshes. No forced garbage collection or working-set trimming is used. Results vary with files, open groups, shell extensions and the machine.

Build a revision with `-Diagnostics` and run `tools/run-performance.ps1 -Executable <absolute-exe-path> -Label <label> -Repeats 2`. The script writes temporary fixtures and JSON results beside itself, without using real groups or desktop files. Averaged values are in [benchmark-results.json](benchmark-results.json).

Checks cover window reuse, visible-row limits, scrolling, targeted watchers, changed thumbnail pixels, cache limits, portrait image dimensions, state flushing, languages and native Program Files protection in x86 processes.

Windows references: [WOW64 implementation](https://learn.microsoft.com/en-us/windows/win32/winprog64/wow64-implementation-details), [shared registry keys](https://learn.microsoft.com/en-us/windows/win32/winprog64/shared-registry-keys).

## Version 4.0 appearance update

Same two scenarios, two fresh processes per scenario, default x86 build:

| Scenario | Private working set | After refresh workload | Three unchanged refreshes |
| --- | ---: | ---: | ---: |
| 4 collapsed groups, 12 files each | 39.47 MB | 55.6 MB | 42.85 ms |
| 4 collapsed groups, 120 files each | 43.52 MB | 57.22 MB | 32.56 ms |

The appearance and rich-name dialogs are created only on demand. Closed panels retain compact model data and frozen vector icons; styled titles use lightweight text runs. Custom images are decoded to at most 96 pixels, with a 24-entry cache; vector icons share a bounded 128-entry cache. Changing preview colors reuses the decoded custom image.

These are idle collapsed-panel measurements, not a fixed cap. Open panels, image thumbnails and editor dialogs can use more memory. The existing virtualization, background loading, window reuse and x86 build are retained. No forced collection or working-set trimming is used. [4.0 results](benchmark-results-4.0.json).

On the existing five-group installation, a fresh 4.0 process with all groups collapsed measured 42.98–43.10 MB private working set. The Explorer pin/unpin check runs in a short-lived helper process, which exits after integration; one resident process remains. This avoids retaining Explorer COM resources in the desktop-panel process. No personal file paths or group names are included in the published measurements.
