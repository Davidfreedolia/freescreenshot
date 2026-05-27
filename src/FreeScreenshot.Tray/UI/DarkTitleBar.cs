using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FreeScreenshot.UI;

/// <summary>
/// Flips the default non-client title bar from Windows' light/accent default
/// to the Windows immersive dark theme. Win10 2004+ supports it (attr 19),
/// Win11 22000+ exposes the same as attr 20.
/// </summary>
internal static class DarkTitleBar
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE     = 20;

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
        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, _) =>
            {
                if (sender is Window w) Apply(w);
            }));
    }
}
