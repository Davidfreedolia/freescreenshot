#!/usr/bin/env bash
# Bulk rename FreeScreenshot -> Freezshot across the codebase, in place.
# Preserves URLs and infra identifiers that legitimately keep the old name:
#   - github.com/Davidfreedolia/freescreenshot           (URL, lowercase)
#   - https://pub-*.r2.dev/Setup_FreeScreenshot.exe       (R2 public URL)
#   - public.freescreenshot_latest                        (Postgres table)
#   - /api/freescreenshot/                                (Vercel rewrites)
# All lowercase "freescreenshot" matches are left intact.
# All "FreeScreenshot" CamelCase matches become "Freezshot".

set -euo pipefail
cd "$(dirname "$0")"

# --- 1. Clone src/FreeScreenshot.* directories to src/Freezshot.* ---
for old in src/FreeScreenshot.Core src/FreeScreenshot.UI src/FreeScreenshot.Tray; do
  new="${old/FreeScreenshot/Freezshot}"
  if [ -d "$new" ]; then rm -rf "$new"; fi
  cp -r "$old" "$new"
done

# --- 2. Rename .csproj filenames inside the new dirs ---
for f in src/Freezshot.Core/FreeScreenshot.Core.csproj \
         src/Freezshot.UI/FreeScreenshot.UI.csproj \
         src/Freezshot.Tray/FreeScreenshot.Tray.csproj; do
  if [ -f "$f" ]; then
    new="${f/FreeScreenshot/Freezshot}"
    mv "$f" "$new"
  fi
done

# --- 3. sed-replace "FreeScreenshot" -> "Freezshot" in source-y files ---
# Scope: .cs, .xaml, .csproj, .sln, .iss, .md (NOT .log, .png, .ico, .exe, .pdb)
mapfile -t files < <(find . \
  \( -path ./.git -o -path ./bin -o -path ./obj -o -path ./publish \
     -o -path ./src/FreeScreenshot.Core -o -path ./src/FreeScreenshot.UI \
     -o -path ./src/FreeScreenshot.Tray \) -prune -o \
  -type f \( -name '*.cs' -o -name '*.xaml' -o -name '*.csproj' -o \
             -name '*.sln' -o -name '*.iss' -o -name '*.md' -o \
             -name '*.html' -o -name '*.svg' -o -name '*.txt' -o \
             -name '*.cmd' \) -print)

for f in "${files[@]}"; do
  # Skip files we don't actually want to touch
  case "$f" in
    *r2up.log|*publish.log|*iscc.log|*git.log) continue ;;
  esac
  # Replace CamelCase only (lowercase 'freescreenshot' is infra and is preserved)
  sed -i 's/FreeScreenshot/Freezshot/g' "$f"
done

# --- 4. Rename the .sln file itself ---
if [ -f FreeScreenshot.sln ]; then
  mv FreeScreenshot.sln Freezshot.sln
fi

# --- 5. Drop the old src/FreeScreenshot.* directories ---
rm -rf src/FreeScreenshot.Core src/FreeScreenshot.UI src/FreeScreenshot.Tray

# --- 6. Clean bin/obj/publish (they reference old assembly names) ---
rm -rf publish
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +

echo "rename done"
