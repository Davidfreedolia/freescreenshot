$ErrorActionPreference = 'Stop'
$root = "C:\Projects\freescreenshoot (1)"

# 1. Copy each FreeScreenshot.* dir to Freezshot.*
foreach ($name in @('Core','UI','Tray')) {
  $src = Join-Path $root "src\FreeScreenshot.$name"
  $dst = Join-Path $root "src\Freezshot.$name"
  if (Test-Path $dst) { Remove-Item -Recurse -Force $dst }
  Copy-Item -Recurse -Force $src $dst
}

# 2. Rename .csproj files
foreach ($name in @('Core','UI','Tray')) {
  $old = Join-Path $root "src\Freezshot.$name\FreeScreenshot.$name.csproj"
  $new = Join-Path $root "src\Freezshot.$name\Freezshot.$name.csproj"
  if (Test-Path $old) { Rename-Item $old $new }
}

# 3. Drop bin/obj inside the new dirs
Get-ChildItem -Path $root\src\Freezshot.* -Recurse -Directory -Force `
  | Where-Object { $_.Name -in @('bin','obj') } `
  | Remove-Item -Recurse -Force

# 4. Replace FreeScreenshot -> Freezshot inside source files
$files = Get-ChildItem -Path $root\src\Freezshot.* -Recurse -File `
  | Where-Object { $_.Extension -in @('.cs','.xaml','.csproj','.manifest') -or $_.Name -eq 'app.manifest' }
foreach ($f in $files) {
  $c = Get-Content $f.FullName -Raw
  $new = $c -replace 'FreeScreenshot','Freezshot'
  if ($new -ne $c) {
    Set-Content -Path $f.FullName -Value $new -NoNewline
  }
}

# 5. Drop old src/FreeScreenshot.* dirs
foreach ($name in @('Core','UI','Tray')) {
  $old = Join-Path $root "src\FreeScreenshot.$name"
  if (Test-Path $old) { Remove-Item -Recurse -Force $old }
}

# 6. Done
"renamed $($files.Count) files"
Get-ChildItem $root\src -Directory -Name
