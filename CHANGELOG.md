# История изменений

## 4.1.3

- Uninstall removes global state, backups, layout cache, queued requests and empty legacy application directories.
- Keeps user group folders, .groups documents and custom icon assets for restoration after reinstall.
- Cleanup runs without starting the desktop UI or recreating settings; it waits for file operations to finish.
- Added preservation, restoration, foreign-file protection and install/uninstall cleanup checks.

## 4.1.2

- Per-user Windows installer with English/Russian UI, shortcuts, Explorer commands and uninstall.
- Uninstall preserves user files and settings and removes only its own shell entries.
- Graceful installer shutdown waits for the app to close and avoids initializing data on a cold exit.
- Added supplied screenshots, short AI disclosure and source-available no-redistribution terms.

## 4.1.1

- Fixed expanded groups becoming invisible after the animation ended.
- Persist animation targets as base values; keep software rendering and released collapsed contents.
- Added checks for visible file rows after first open, rapid toggles and asynchronous refresh.

## 4.1.0

- Software rendering for the small desktop UI, avoiding high graphics-driver memory use on the test PC.
- In-place appearance updates, released collapsed content trees and explicit dialog-content cleanup.
- Finite animations with stable base values and a 45 fps ceiling.
- Kept language, file operations, portable group documents, colors, icons and rich titles.
- Excluded checks, benchmarks and demo generators from normal builds; use `-Diagnostics` for developer tools.
- Added visible-window memory workload and tests for in-place styling, contrast and interactive reopening.

## 4.0.0

- Independent 30-color icon/header and background palettes for each group.
- Fifteen matching vector icons and proportional custom image import, portable with the group folder.
- Selected-name text formatting in Rename: 30 colors, bold, italic, underline and reset.
- Rounded buttons with vector action icons and finite hover/press animations.
- Compact persisted title runs; dialogs created only when opened, bounded icon caches and proportional image decoding.
- Explorer integration runs in a short-lived worker, releasing its COM resources after startup.
- Added Apply/Cancel, styled rename, image portability and metadata exclusion checks.

## 3.2.0

- Lazy group contents, visible-row rendering and bounded background icon loading.
- Reused windows and watchers, targeted updates and batched state saves.
- Bounded thumbnails, refreshed image pixels after modification and safe flushing on exit.
- Default x86 build with optional x64 and native Program Files protection.
- Added scrolling/resource checks and a reproducible performance benchmark.

## 3.1.0

- Added English/Russian interface switching from the notification-area menu.
- Saved language preferences and localized panels, tooltips, app dialogs and Explorer commands.
- Kept user group names, files and folder locations unchanged when switching languages.
- Added English and Russian documentation with real-panel demo previews.
## 3.0.0

- Группы работают на одном обычном рабочем столе Windows.
- Файлы хранятся в обычных папках Проводника; на рабочем столе отображаются панели групп.
- Удалены страницы, стрелки переключения, формат `.desk` и захват скриншотов.
- Сохранены перетаскивание файлов, меню Windows, автозапуск и формат `.groups`.
- Добавлено обновление старых данных с сохранением файлов и восстановлением прерванных перемещений.

## Ранние версии

Ранние локальные версии использовали дополнительные страницы и миниатюры скриншотов. Эти возможности удалены в 3.0.0.
