"""
Render brand/icon.svg into a multi-resolution Windows .ico.
Used to keep brand/icon.ico and src/FreeScreenshot.Tray/Resources/app.ico in sync.

Requires: cairosvg, Pillow. Install with:
    py -m pip install cairosvg pillow --break-system-packages
"""
from io import BytesIO
from pathlib import Path

import cairosvg
from PIL import Image

HERE = Path(__file__).resolve().parent
SVG = HERE / "icon.svg"
OUT_ICO = HERE / "icon.ico"
SIZES = [16, 24, 32, 48, 64, 128, 256]

imgs = []
for size in SIZES:
    png_bytes = cairosvg.svg2png(
        url=str(SVG), output_width=size, output_height=size
    )
    img = Image.open(BytesIO(png_bytes)).convert("RGBA")
    imgs.append(img)

# Save the largest as ICO with all sizes embedded
imgs[-1].save(
    OUT_ICO,
    format="ICO",
    sizes=[(s, s) for s in SIZES],
    append_images=imgs[:-1],
)
print(f"wrote {OUT_ICO} with sizes {SIZES}")
