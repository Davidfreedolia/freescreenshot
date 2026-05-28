# Freezshot — project log

Chronicle of what we built, by version. Lives next to the code so any
future agent (or future me) can recover context fast without re-reading
the entire conversation history.

> Versions before 2.0 shipped under the old name **FreeScreenshot**.
> v2.0 is the rebrand to **Freezshot**.

---

## Phase 0 — Scaffolding (pre-release)

- Stack chosen: .NET 8 / WPF + WinForms hybrid, Inno Setup 6, Supabase
  Edge Functions + Postgres, Cloudflare R2 for binaries, Vercel
  rewrites proxying `freedolia.com/api/freescreenshot/*`.
- License: GPLv3.
- Modular solution layout: `Core` (config, telemetry, i18n, capture
  primitives), `UI` (design tokens, shared styles), `Tray`
  (executable host + WPF windows).
- Brand: "FreeScreenshot" with a lime accent (later replaced).
- Repo created at `github.com/Davidfreedolia/freescreenshot`.
- Hello-world WPF compiled, GitHub Actions build, OSS docs (README,
  LICENSE, PRIVADESA.md), `setup.ps1`.

## Phase 1 — Backend + telemetry

- Postgres schema for `leads`, `installs`, `uninstall_feedback`,
  `freescreenshot_latest` (single-row "latest version" table).
- Four Supabase Edge Functions: `download`, `install`, `latest`,
  `uninstall`.
- `TelemetryClient` in `Core` — fail-silent HTTP wrapper around the
  four endpoints, gated on user opt-in.
- Privacy policy drafted (PRIVADESA.md). README updated to be honest
  about what's collected and when.

## Phase 2 — Tray-first UX

- Single instance via `Global\Freedolia.FreeScreenshot.Singleton`
  named mutex.
- `TrayHost` with `System.Windows.Forms.NotifyIcon` (replacing
  `H.NotifyIcon` which had a `UriFormatException` bug).
- Settings, Privacy, About windows. Update-available dialog with
  Update / Later / Skip buttons.

## Phase 3 — Installer + distribution

- Inno Setup script (`Setup_FreeScreenshot.iss`) with per-user install
  at `%LOCALAPPDATA%\Programs\FreeScreenshot`.
- Catalan/Spanish/English language pages.
- Autostart task (unchecked by default).
- Uninstall feedback hook: runs the app with `--uninstall-feedback`
  before files are removed, app shows a small survey, posts the reason.
- Distribution via Cloudflare R2, fronted by Vercel rewrites under
  `freedolia.com/api/freescreenshot/*`.

## v1.0 — Real screen capture

- GDI capture via `Graphics.CopyFromScreen`, fullscreen virtual desktop
  support, save to `Pictures/FreeScreenshot/` by default.
- First fullscreen overlay (dim panels, selection rectangle, dimensions
  chip).

## v1.1 — Onboarding + multi-monitor + history + folder config

- First-launch onboarding window (three steps with illustrations).
- Multi-monitor capture using `SystemParameters.VirtualScreen*`.
- Tray menu "recent captures" submenu (last 8).
- Configurable output folder.

## v1.2 — Editor + fullscreen capture + mutex fix

- First version of `EditorWindow` (basic arrows, rectangles, text).
- Fullscreen mode (`Ctrl+Shift+3`).
- Mutex `ReleaseMutex` bug fixed via `_mutexOwned` flag.

## v1.3 — Floating toolbar + pin + OCR

- CleanShot-style floating toolbar after a capture: Editor, Pin, OCR,
  Save, Copy.
- `PinnedWindow` — topmost floating thumbnail, click-drag to move.
- OCR via `Windows.Media.Ocr`.

## v1.4 — Dark title bars + layout fix + icon refresh

- `DarkTitleBar.HookAll()` — class handler that applies
  `DWMWA_USE_IMMERSIVE_DARK_MODE` to every window.
- Settings layout fixed (ScrollViewer + resize).
- Icon redrawn with lime gradient + dark brackets.

## v1.5 — Hint copy + pinned grid + editor polish

- Selection hint becomes a plain TextBlock with drop shadow (was an
  ugly pill).
- Pinned screenshots auto-arrange in the bottom-right at uniform
  220px width via a static `_alive` list and `Reflow()`.
- Editor gets a color picker (5 swatches), stroke width selector,
  icon-only tool buttons.

## v1.6 — Onboarding illustrations + show-once

- Onboarding now has SVG illustrations per step (taskbar, selection,
  folder).
- Marked done immediately on show (no re-trigger on every boot).
- Re-showable from Settings → About.

## v1.7 — Blur + crop + window capture

- Editor adds Blur (WPF `BlurEffect`) and Crop (`CroppedBitmap`).
- `WindowFinder` uses `GetForegroundWindow` + `DwmGetWindowAttribute`
  (`DWMWA_EXTENDED_FRAME_BOUNDS`) for accurate window bounds.

## v1.8 — Numbers, marker, line, output format

- Editor adds Number, Marker, Line tools.
- Output format toggle (PNG/JPG) in Settings.

## v1.9 — Preview thumbnail post-capture

- `CapturePreviewWindow`: macOS-style preview shelf in the
  bottom-right, fades in, lingers ~5s, fades out. Click to open the
  file. Hover to pause the timer. Stacks vertically when multiple
  previews are alive.
- Config flag `ShowPreview` + toggle in Settings.
- Defensive `static App()` env restore for `WINDIR`/`SystemRoot` —
  otherwise WPF's `FontCache.Util` cctor throws `UriFormatException`
  when launched from PowerShell sessions that strip those vars.
- Dark ComboBox `ControlTemplate` written from scratch (the default
  template ignores `Foreground`).
- `SetWindowPos SWP_FRAMECHANGED` after `DwmSetWindowAttribute` so
  the title bar actually refreshes after being darkened.

## v1.10 — Freedolia rebrand (palette + icon)

- Palette swapped lime → Freedolia teal:
  `Accent.Base #2DD4BF`, `Hover #5EEAD4`, `Pressed #14B8A6`.
- Backgrounds re-toned to deep teal-tinted dark.
- Generous CornerRadius tokens (`Sm 8`, `Md 12`, `Lg 18`, `Pill 999`).
- New `FsPrimaryButton` (teal pill) + custom focus ring (the WPF
  purple dotted rectangle is gone).
- Icon redrawn: brackets + center dot on a rounded dark tile,
  teal gradient stroke. Multi-resolution `.ico` generated from the
  SVG via Pillow.
- Floating toolbar, pinned border, selection overlay, preview shelf
  all repainted teal-coherent.
- **Click a preview/history item → opens our editor**, never Paint.
  Implemented via `EditorWindow.OpenForFile(path)`.
- Landing pages (CA/ES/EN) + home banner + handover doc for the
  freedolia-hub cowork created in `web/`.

## v1.10.x — Polish

- Fix: floating toolbar was rendering as a flat ellipse because
  `CornerRadius=999` exceeded the half-height. Replaced with `26` on
  the outer border + `19` on each icon button, plus more breathing
  room (`Padding=8,7`, larger button margins).

## v2.0 — Rename to Freezshot

- Renamed every UI-facing surface and code-level identifier:
  - Projects: `FreeScreenshot.{Core,UI,Tray}` → `Freezshot.{…}`.
  - Solution: `FreeScreenshot.sln` → `Freezshot.sln`.
  - Namespaces, assembly names, root namespaces.
  - Inno Setup: new `AppId`, new `OutputBaseFilename=Setup_Freezshot`,
    install dir `%LOCALAPPDATA%\Programs\Freezshot`.
  - Mutex: `Global\Freedolia.Freezshot.Singleton`.
  - Config dir: `%LOCALAPPDATA%\Freezshot`.
  - Registry Run value name: `Freezshot`.
  - Wordmark SVG text: `Freezshot`.
  - Landing/banner copy + URLs `/{ca,es,en}/freezshot`.
- Infra **kept under the old name** intentionally (zero downtime):
  - GitHub repo URL `…/freescreenshot`.
  - R2 bucket `freescreenshot` (the object inside is now
    `Setup_Freezshot.exe`).
  - Vercel rewrites `/api/freescreenshot/*`.
  - Postgres table `public.freescreenshot_latest`.
- Old `FreeScreenshot.Tests` project removed (was empty stubs).
- Modular layout preserved — adding a future module is `dotnet new
  classlib -o src/Freezshot.<Module>` + a `ProjectReference` in the
  `Tray` csproj. The pattern Core → UI → Tray is enforced by the
  reference graph (Tray references UI and Core; UI references Core;
  Core is leaf).
- Bumped to v2.0.0.
- AGENTS.md + PROJECT_LOG.md added.

## v2.0.1 — Button polish

- `FsPrimaryButton` was rendering as a flat ellipse because
  `CornerRadius=999` over a sub-36px button is interpreted by WPF as a
  full ellipse. Replaced with `CornerRadius=18`, `MinHeight=36`,
  `Padding=20,9` so the pill shape is correct at any size.
- Editor toolbar (`Save`, `Copy`, `Cancel`) was overweight. Now:
  - `SaveBtn` uses the shared `FsPrimaryButton` style (consistent teal
    pill, correct `OnBase` text colour, no hard-coded `#FF1A1814`).
  - `CopyBtn` reduced to `Padding=14,7`, `MinWidth=92`.
  - `CancelBtn` reduced to `Padding=12,6`, `FontSize=13`.
  - All three set `FocusVisualStyle=null` to suppress the purple WPF
    focus rectangle on click.

## Process rule (effective v2.0.1+)

Every commit from here on **must** update `AGENTS.md` and
`PROJECT_LOG.md` before being pushed. See the "House rule" at the top
of `AGENTS.md` for the checklist.

## v2.1 — Clean upgrade migration, visible donation, SmartScreen guidance

- **Upgrade migration in Inno Setup `[Code]` section.** When you install
  Freezshot it now scans `HKCU\…\Uninstall\*` and `HKLM\…\Uninstall\*`
  by `DisplayName="FreeScreenshot"`, runs the old uninstaller if its
  exe still exists, and deletes the orphan registry key + leftover
  install folder `%LOCALAPPDATA%\Programs\FreeScreenshot` either way.
  Verified: machine ends with a single "Freezshot 2.1.0" entry in
  Add/Remove Programs even when the old install was corrupt or
  partially removed.
- **Why enumeration, not a fixed key.** The first attempt hard-coded
  `{6E2B1D2A-…}_is1` as the path. Inno writes the actual key as
  `{6E2B1D2A-…}}_is1` (extra `}`) so a literal-path match silently
  failed. We now enumerate subkeys and match by `DisplayName`. Robust
  to any Inno key-naming change.
- **Donation surfaced.** Tray context menu now has a bold "❤  Fer un
  donatiu" entry promoted above Quit. Onboarding gains a footer with
  the same CTA (outlined teal button). Both point to the Stripe
  payment link.
- **Stripe link corrected.** `Strings.DonationUrl` was a placeholder
  `freedolia.com/donate`. Now it's the real Stripe Payment Link
  shared with the FreeWisp landing flow:
  `https://donate.stripe.com/6oUcN559jeWpcDZ8eMfYY04`.
- **SmartScreen pre-warning on landing.** Each landing (`/ca/`, `/es/`,
  `/en/`) shows a small note below the download button explaining
  "Windows protected your PC → More info → Run anyway", because we
  don't carry a paid code-signing certificate. Same teal `--accent`
  styling.
- **Lead-capture flow documented.** `web/HANDOVER.md` section 5 spells
  out the same pattern as FreeWisp: email modal → `public.leads` row
  with `product='freezshot'` → R2 download → donation message on the
  thank-you page.

---

## Operational notes

- **Build:** `dotnet publish` self-contained single-file
  (`PublishSingleFile=true` + `IncludeNativeLibrariesForSelfExtract=true`).
  Do not enable `EnableCompressionInSingleFile` — broke font cache
  during testing.
- **Install path:** per-user, no elevation. Same model as FreeWisp.
- **Telemetry:** opt-in only. Install ID is a per-install UUID stored
  in `config.json`. Nothing else is sent unless the user opts in to
  analytics from Settings → Privacy.
- **WPF env quirk:** WPF `FontCache.Util` requires `WINDIR`/`SystemRoot`
  in the process env. `static App()` repopulates them defensively.

## Things we deliberately didn't ship (kept for later)

- Configurable hotkeys (currently hard-wired Ctrl+Shift+1/2/3).
- Scroll-capture for long pages.
- Video / GIF recording.
- Cloud upload (Imgur etc.).
- Migration path that auto-copies the old FreeScreenshot config into
  the new Freezshot location. (No real users to migrate yet.)
