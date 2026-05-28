#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

# Wipe stale Freezshot.* dirs (incomplete from earlier attempt)
rm -rf src/Freezshot.Core src/Freezshot.UI src/Freezshot.Tray

# Clone src/FreeScreenshot.* to src/Freezshot.* in full
for old in src/FreeScreenshot.Core src/FreeScreenshot.UI src/FreeScreenshot.Tray; do
  new="${old/FreeScreenshot/Freezshot}"
  cp -r "$old" "$new"
done

# Rename .csproj files
mv src/Freezshot.Core/FreeScreenshot.Core.csproj src/Freezshot.Core/Freezshot.Core.csproj
mv src/Freezshot.UI/FreeScreenshot.UI.csproj     src/Freezshot.UI/Freezshot.UI.csproj
mv src/Freezshot.Tray/FreeScreenshot.Tray.csproj src/Freezshot.Tray/Freezshot.Tray.csproj

# Wipe bin/obj inside the new dirs
find src/Freezshot.Core src/Freezshot.UI src/Freezshot.Tray -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true

# sed-replace FreeScreenshot -> Freezshot in all source-ish files inside the NEW dirs
find src/Freezshot.Core src/Freezshot.UI src/Freezshot.Tray -type f \
  \( -name '*.cs' -o -name '*.xaml' -o -name '*.csproj' -o -name '*.manifest' -o -name 'app.manifest' \) \
  -print0 | xargs -0 sed -i 's/FreeScreenshot/Freezshot/g'

# Drop the old src/FreeScreenshot.* dirs
rm -rf src/FreeScreenshot.Core src/FreeScreenshot.UI src/FreeScreenshot.Tray

echo "done"
