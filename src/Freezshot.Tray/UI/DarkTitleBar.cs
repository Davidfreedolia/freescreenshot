using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Freezshot.UI;

/// <summary>
/// Flips the default non-client title bar from Windows' light/accent default
/// to the Windows immersive dark theme. Win10 2004+ supports it (attr 19),
/// Win11 22000+ exposes the same as attr 20.
/// </summary>
internal static class DarkTitleBar
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE     = 20;

    private const uint SWP_NOSIZE         = 0x0001;
    private const uint SWP_NOMOVE         = 0x0002;
    private const uint SWP_NOZORDER       = 0x0004;
    private const uint SWP_FRAMECHANGED   = 0x0020;
    private const uint SWP_NOACTIVATE     = 0x0010;

    /// <summary>Apply dark title bar to a window. Safe to call at any point in its lifetime.</summary>
    public static void Apply(Window window)
    {
        void Set()
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;
            int on = 1;
            // Try modern Win11 attribute first; if it fails, fall back to Win10 2004.
            if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref on, sizeof(int));
            }
            // Force the non-client area to redraw with the new attribute. Without this,
            // setting the attribute after the window has already shown leaves the
            // original light/accent title bar visible until the window is moved/resized.
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            Set();
        }
        else
        {
            window.SourceInitialized += (_, _) => Set();
        }
    }

    /// <summary>Hook every Window in the app at construction time. Call once in App startup.</summary>
    public static void HookAll()
    {
        // Class handler for Window.Initialized would be ideal but Initialized
        // is not a routed event. Loaded is the earliest routed event we can
        // class-handle; we compensate by forcing a frame-change redraw inside
        // Apply() so the title bar refreshes after the attribute is set.
        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, _) =>
            {
                if (sender is Window w) Apply(w);
            }));
    }
}
