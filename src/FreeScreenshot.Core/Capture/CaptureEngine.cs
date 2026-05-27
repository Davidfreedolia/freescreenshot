using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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
    public static string SaveToDisk(byte[] pngBytes, string? folder = null)
    {
        folder ??= DefaultSaveFolder;
        Directory.CreateDirectory(folder);
        var now = DateTime.Now;
        var name = $"screenshot-{now:yyyy-MM-dd_HH-mm-ss}.png";
        var path = Path.Combine(folder, name);
        File.WriteAllBytes(path, pngBytes);
        return path;
    }
}
