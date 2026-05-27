#Requires -Version 5.1
<#
.SYNOPSIS
  FreeScreenshot — first-time repository setup.

.DESCRIPTION
  Downloads the GPLv3 LICENSE from gnu.org, initializes git, runs an initial
  build to verify everything compiles, makes the first commit, and prints
  instructions to push to GitHub.

  Safe to run multiple times: each step checks state before acting.

.EXAMPLE
  pwsh ./setup.ps1
#>

[CmdletBinding()]
param(
  [string] $GitHubRemoteUrl = "",
  [switch] $SkipBuild,
  [switch] $SkipLicenseDownload
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

# -------- helpers --------
function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Green }
function Write-Info($msg) { Write-Host "    $msg" -ForegroundColor Gray }
function Write-Warn($msg) { Write-Host "!!  $msg" -ForegroundColor Yellow }
function Write-Fail($msg) { Write-Host "XX  $msg" -ForegroundColor Red }

function Test-Command($name) {
  $null -ne (Get-Command $name -ErrorAction SilentlyContinue)
}

# -------- preflight --------
Write-Host ""
Write-Host "FreeScreenshot — setup" -ForegroundColor Green
Write-Host "A Freedolia project · GPLv3" -ForegroundColor DarkGray
Write-Host ""

Write-Step "Checking prerequisites"

if (-not (Test-Command "git")) {
  Write-Fail "git not found. Install from https://git-scm.com/download/win and retry."
  exit 1
}
Write-Info "git found: $(git --version)"

if (-not (Test-Command "dotnet")) {
  Write-Fail ".NET SDK not found. Install .NET 8 SDK from https://dotnet.microsoft.com/download and retry."
  exit 1
}
$dotnetVer = (dotnet --version)
Write-Info ".NET SDK found: $dotnetVer"

$major = [int]($dotnetVer.Split(".")[0])
if ($major -lt 8) {
  Write-Warn ".NET SDK is older than 8.0. Build may fail."
}

# -------- LICENSE --------
Write-Step "Fetching GPLv3 LICENSE from gnu.org"
$licensePath = Join-Path $root "LICENSE"
$placeholderMarker = "SPDX-License-Identifier: GPL-3.0-or-later"

$alreadyFull = $false
if (Test-Path $licensePath) {
  $content = Get-Content -Raw -Path $licensePath
  if ($content.Length -gt 20000) { $alreadyFull = $true }
}

if ($SkipLicenseDownload) {
  Write-Info "Skipping (per flag)."
} elseif ($alreadyFull) {
  Write-Info "LICENSE already contains the full text. Skipping."
} else {
  try {
    $url = "https://www.gnu.org/licenses/gpl-3.0.txt"
    $tmp = New-TemporaryFile
    Invoke-WebRequest -Uri $url -OutFile $tmp.FullName -UseBasicParsing
    $licenseText = Get-Content -Raw -Path $tmp.FullName
    Remove-Item $tmp.FullName

    # Prepend project notice + then the canonical GPL text
    $header = @"
FreeScreenshot — Copyright (C) 2026 Freedolia and contributors.

This file contains the authoritative text of the GNU General Public License
version 3.0 as published by the Free Software Foundation.

SPDX-License-Identifier: GPL-3.0-or-later

-------------------------------------------------------------------------------

"@
    Set-Content -Path $licensePath -Value ($header + $licenseText) -Encoding UTF8 -NoNewline
    Write-Info "LICENSE written ($([math]::Round((Get-Item $licensePath).Length / 1KB)) KB)."
  }
  catch {
    Write-Warn "Could not download LICENSE: $($_.Exception.Message)"
    Write-Warn "You'll need to download https://www.gnu.org/licenses/gpl-3.0.txt manually."
  }
}

# -------- git init --------
Write-Step "Initializing git repository"

if (Test-Path ".git") {
  Write-Info "Repository already initialized."
} else {
  git init | Out-Null
  git branch -M main
  Write-Info "Repository created on 'main' branch."
}

# -------- restore + build --------
if (-not $SkipBuild) {
  Write-Step "Restoring NuGet packages"
  dotnet restore FreeScreenshot.sln
  if ($LASTEXITCODE -ne 0) { Write-Fail "Restore failed."; exit 1 }

  Write-Step "Building solution (Release)"
  dotnet build FreeScreenshot.sln --configuration Release --no-restore
  if ($LASTEXITCODE -ne 0) { Write-Fail "Build failed."; exit 1 }

  Write-Step "Running tests"
  dotnet test FreeScreenshot.sln --configuration Release --no-build --verbosity minimal
  if ($LASTEXITCODE -ne 0) { Write-Warn "Some tests failed — investigate before pushing." }
} else {
  Write-Info "Skipping build (per flag)."
}

# -------- first commit --------
Write-Step "Staging initial commit"

git add . | Out-Null
$status = git status --porcelain
if ([string]::IsNullOrWhiteSpace($status)) {
  Write-Info "Nothing to commit — working tree clean."
} else {
  git -c "user.name=Freedolia Bot" -c "user.email=setup@freedolia.com" `
      commit -m "chore: initial scaffolding (FreeScreenshot v0.0.1)" `
              -m "See ROADMAP.md and DESIGN-SYSTEM.md for the plan." | Out-Null
  Write-Info "Initial commit created."
  Write-Warn "Set your real git identity before further commits:"
  Write-Warn "    git config user.name  ""Your Name"""
  Write-Warn "    git config user.email ""you@example.com"""
}

# -------- remote --------
if (-not [string]::IsNullOrWhiteSpace($GitHubRemoteUrl)) {
  Write-Step "Adding remote 'origin'"
  $existing = (git remote get-url origin 2>$null)
  if ($LASTEXITCODE -eq 0) {
    git remote set-url origin $GitHubRemoteUrl
    Write-Info "Remote updated → $GitHubRemoteUrl"
  } else {
    git remote add origin $GitHubRemoteUrl
    Write-Info "Remote added → $GitHubRemoteUrl"
  }
  Write-Step "Pushing to GitHub"
  git push -u origin main
} else {
  Write-Host ""
  Write-Host "Next steps to publish to GitHub:" -ForegroundColor Green
  Write-Host ""
  Write-Host "  1. Create an empty repo on GitHub (organization 'freedolia', name 'freescreenshot')."
  Write-Host "     Do NOT initialize it with README, LICENSE, or .gitignore — we already have those."
  Write-Host ""
  Write-Host "  2. Connect and push:"
  Write-Host "       git remote add origin https://github.com/freedolia/freescreenshot.git"
  Write-Host "       git push -u origin main"
  Write-Host ""
  Write-Host "     Or re-run this script with the remote URL:"
  Write-Host "       pwsh ./setup.ps1 -GitHubRemoteUrl https://github.com/freedolia/freescreenshot.git"
  Write-Host ""
}

Write-Step "Done."
Write-Host ""
Write-Host "To run the Hello World window:" -ForegroundColor Green
Write-Host "    dotnet run --project src/FreeScreenshot.Tray"
Write-Host ""
