using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace FreeScreenshot.Core.Capture;

/// <summary>
/// GDI-based screen capture. Simple, fast, works on every Windows since
/// Windows XP. Good enough for v1.0 (single monitor, single-DPI per capture).
/// Multi-monitor + DPI-aware capture is a v1.1 concern.
/// </summary>
public static class GdiCaptureEngine
{
    /// <summary>Capture a region of the screen in physical pixels and return PNG bytes.</summary>
    public static byte[] CapturePng(int physicalX, int physicalY, int physicalWidth, int physicalHeight)
    {
        if (physicalWidth < 1 || physicalHeight < 1)
            throw new ArgumentException("Capture region must be at least 1x1.");

        using var bmp = new Bitmap(physicalWidth, physicalHeight, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(physicalX, physicalY, 0, 0, new Size(physicalWidth, physicalHeight));
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    /// <summary>Default save folder under the user's Pictures directory.</summary>
    public static string DefaultSaveFolder
    {
        get
        {
            var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            return Path.Combine(pictures, "FreeScreenshot");
        }
    }

    /// <summary>Save PNG bytes to disk with a timestamp-based filename. Returns full path.</summary>
    public static string SaveToDisk(byte[] pngBytes, string? folder = null, string format = "png", int jpegQuality = 90)
    {
        folder ??= DefaultSaveFolder;
        Directory.CreateDirectory(folder);
        var now = DateTime.Now;
        var fmt = (format ?? "png").ToLowerInvariant();
        var ext = fmt switch { "jpg" or "jpeg" => "jpg", _ => "png" };
        var name = $"screenshot-{now:yyyy-MM-dd_HH-mm-ss}.{ext}";
        var path = Path.Combine(folder, name);

        if (ext == "png")
        {
            File.WriteAllBytes(path, pngBytes);
        }
        else
        {
            // Re-encode the PNG bytes through GDI+ to JPEG.
            using var src = System.Drawing.Image.FromStream(new MemoryStream(pngBytes));
            var jpgEncoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.MimeType == "image/jpeg");
            if (jpgEncoder is null)
            {
                File.WriteAllBytes(path.Replace(".jpg", ".png"), pngBytes);
            }
            else
            {
                var p = new System.Drawing.Imaging.EncoderParameters(1);
                p.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, (long)Math.Clamp(jpegQuality, 50, 100));
                src.Save(path, jpgEncoder, p);
            }
        }
        return path;
    }
}
