using System.Runtime.InteropServices;

namespace FreeScreenshot.Capture;

/// <summary>
/// Tiny Win32 wrapper that returns the foreground window's actual visible
/// bounds in physical pixels. Uses DWMWA_EXTENDED_FRAME_BOUNDS so we exclude
/// the invisible drop-shadow margin that GetWindowRect would include.
/// </summary>
internal static class WindowFinder
{
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT r);
    [DllImport("dwmapi.dll")] private static extern int  DwmGetWindowAttribute(IntPtr hWnd, int attr, out RECT pvAttr, int cbAttr);

    private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    /// <summary>Foreground window rect in physical pixels. Null if no foreground window.</summary>
    public static (int X, int Y, int W, int H)? GetForegroundRect()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return null;

        // Try DWM extended frame bounds first (accurate on Win10/11 with composition).
        if (DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out var r, Marshal.SizeOf<RECT>()) == 0
            && r.Right > r.Left && r.Bottom > r.Top)
        {
            return (r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }
        // Fallback.
        if (GetWindowRect(hwnd, out var r2))
        {
            return (r2.Left, r2.Top, r2.Right - r2.Left, r2.Bottom - r2.Top);
        }
        return null;
    }
}
