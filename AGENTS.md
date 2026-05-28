# AGENTS.md — agents & skills used on Freezshot

Reference of every AI agent, skill and external tool used to build and
ship Freezshot (the Freedolia screenshot tool for Windows). Format follows
the agents.md convention so a future agent walking into this repo knows
which capabilities the project leans on.

## Project at a glance

- **Product:** Freezshot — Windows screenshot tool with editor, OCR, pin,
  blur, multi-monitor, tray-first UX. CleanShot-X-inspired.
- **Brand:** Freedolia. Visual language matches FreeWisp (sister product).
- **License:** GPLv3.
- **Repo:** github.com/Davidfreedolia/freescreenshot (renamed internally
  to Freezshot in code; GitHub slug kept for infra continuity).
- **Stack:** .NET 8 (WPF + WinForms hybrid) · Inno Setup 6 · Supabase
  (Edge Functions + Postgres) · Cloudflare R2 · Vercel rewrites on
  `freedolia.com`.
- **Structure:** modular .NET solution with three projects:
  - `src/Freezshot.Core` — config, telemetry, capture primitives, i18n.
  - `src/Freezshot.UI` — design tokens, shared styles, reusable visuals.
  - `src/Freezshot.Tray` — WPF/tray host, windows, hotkeys, editor.

---

## Agents used

### `general-purpose`
- **Where:** ad-hoc multi-file refactors, cross-file searches, code
  spelunking when the next move wasn't obvious yet.
- **Why:** sandbox for exploration without spending the main loop's
  context window.

### `Explore` (read-only search)
- **Where:** finding all references to a symbol before rename, locating
  string keys before edits.
- **Why:** much faster than chaining Grep + Glob manually.

### `Plan`
- **Where:** sequencing the multi-step rebrand from FreeScreenshot →
  Freezshot (which files touch what, what infra stays).
- **Why:** keeps risky cross-cutting changes from going off the rails.

### Brand-voice subagents (`brand-voice:*`)
- **Where:** copywriting for landing pages, banner, settings strings,
  onboarding wizard. Available but mostly we hand-wrote the copy in
  Catalan/Spanish/English following the established Freedolia tone.

---

## Skills used

### `productivity:task-management`
The task list in this conversation is the master plan-of-record. Every
non-trivial step gets a TaskCreate + TaskUpdate. Used continuously.

### `productivity:memory-management`
Long-running project memory (decisions, why we made certain calls).
The `freescreenshot-decisions.md` memory entry is the source of truth for
naming, stack, license, monetization model and legal posture.

### `pdf` / `pptx` / `docx` / `xlsx`
Loaded on demand when generating decks or report exports. Not core to
the app but available for one-off deliverables.

### `mcp-builder`
Not used directly but listed because future Freezshot MCP integration
(headless capture API) would start from this skill.

### `canvas-design` & `brand-guidelines`
Reference when designing the icon and wordmark — we ended up hand-coding
the SVG to match the FreeWisp aesthetic exactly.

### `web-artifacts-builder`
Used for landing-page scaffolding and the banner mockup that gets
handed to the freedolia-hub cowork.

### `theme-factory`
Reference for token-based theming; informed the structure of
`Freezshot.UI/Resources/DesignTokens.xaml`.

---

## External MCPs / tools used

| Tool                              | Why                                                              |
| --------------------------------- | ---------------------------------------------------------------- |
| `mcp__workspace__bash`            | Linux-side scripts (rename pass, icon generation via cairosvg).  |
| `mcp__Desktop_Commander__*`       | Long-running Windows commands (dotnet publish, ISCC, wrangler).  |
| `mcp__515a94bb-…__execute_sql`    | Supabase Management API — updating `freescreenshot_latest`.      |
| `mcp__computer-use__*`            | Visual verification: launching the app, taking screenshots,      |
|                                   | bringing windows to front to inspect the WPF UI.                 |
| `mcp__visualize__show_widget`     | Rendering the home-banner preview inline for quick review.       |
| `mcp__cowork__present_files`      | Surfacing the final installer + landing files to the user.       |
| Cloudflare R2 (via wrangler.cmd)  | Hosting `Setup_Freezshot.exe`.                                   |
| GitHub (HTTPS push)               | Source of truth: `github.com/Davidfreedolia/freescreenshot`.     |

---

## Build & release dependencies

- **.NET 8 SDK** — installed at `C:\Users\David\.dotnet\dotnet.exe`
  (NOT `C:\Program Files\dotnet\dotnet.exe` — that one only ships the
  runtime).
- **Inno Setup 6** — at `%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe`.
- **Node + Wrangler** — Cloudflare R2 uploads, invoked via the cached
  npx path at `…\npm-cache\_npx\…\node_modules\wrangler\bin\wrangler.js`.
- **Git** — at `C:\Program Files\Git\bin\git.exe`.
- **Python + Pillow + cairosvg** — `brand/make-ico.py` to render the
  multi-resolution `.ico` from the SVG mark.

## Release pipeline (single-keystroke order)

1. `build-fs-v19.cmd` — `dotnet publish` self-contained single-file.
2. `iscc-fs-v19.cmd` — Inno Setup compiles `Setup_Freezshot.exe`.
3. Uninstall old + install new locally; verify with the in-process tray.
4. `upload-fs-v19.cmd` — `wrangler r2 object put` to bucket
   `freescreenshot` as `Setup_Freezshot.exe`.
5. `execute_sql` — bump `public.freescreenshot_latest` row.
6. `commit-fs-v19.cmd` — git commit + push to `main`.

## Defensive patterns worth remembering

- **WINDIR env restore.** `static App()` re-populates `WINDIR`,
  `SystemRoot` and `windir` from the machine env. Without this, WPF's
  `FontCache.Util` cctor throws `UriFormatException` and the whole app
  fails to start when launched from environments that strip those vars
  (some PowerShell sessions, certain remote launchers).
- **PublishSingleFile + IncludeNativeLibrariesForSelfExtract**. Don't
  use `IncludeAllContentForSelfExtract` here — it breaks DPI/font
  resolution. The GitHub release workflow is the source of truth for
  publish flags.
- **Single-instance mutex.** `Global\Freedolia.Freezshot.Singleton` plus
  a `_mutexOwned` bool so we never call `ReleaseMutex` on a non-owned
  mutex (otherwise `ApplicationException`).
- **Open captures in our editor, never Paint.** `EditorWindow.OpenForFile`
  is the single entry point for "click a thumbnail / history item".
