using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeScreenshot.Core.Capture;
using FreeScreenshot.Core.Localization;
using FreeScreenshot.Hotkeys;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;

namespace FreeScreenshot.Capture;

/// <summary>
/// Owns the global hotkey + selection overlay + capture engine.
/// One instance per app, created at startup, disposed on exit.
/// </summary>
internal sealed class CaptureManager : IDisposable
{
    // Default shortcut: Ctrl + Shift + 1. Can be made user-configurable later.
    private const uint VK_1 = 0x31;

    private readonly GlobalHotkey _hotkeys = new();
    private readonly Action<string, string> _toast;
    private bool _capturing;
    private bool _disposed;

    public CaptureManager(Action<string, string> showToast)
    {
        _toast = showToast;
        var ok = _hotkeys.Register(
            GlobalHotkey.Modifiers.Ctrl | GlobalHotkey.Modifiers.Shift,
            VK_1,
            OnHotkey);
        if (ok == 0)
        {
            // Another app already owns Ctrl+Shift+1. Tell the user, but don't crash.
            _toast(Strings.T("capture.error.title"), Strings.T("capture.error.hotkey_busy"));
        }
    }

    private void OnHotkey()
    {
        if (_capturing) return;
        _capturing = true;
        try
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(RunCaptureFlow));
        }
        catch
        {
            _capturing = false;
        }
    }

    private void RunCaptureFlow()
    {
        try
        {
            var overlay = new SelectionOverlay();
            var ok = overlay.ShowDialog();
            if (ok != true || overlay.Selection is not { } sel || sel.Width < 1 || sel.Height < 1)
            {
                return;
            }

            // DIPs → physical pixels.
            var dpi = VisualTreeHelper.GetDpi(overlay).PixelsPerDip;
            var px = (int)Math.Round(sel.X * dpi);
            var py = (int)Math.Round(sel.Y * dpi);
            var pw = (int)Math.Round(sel.Width * dpi);
            var ph = (int)Math.Round(sel.Height * dpi);

            // Give the OS one frame to actually paint the overlay away
            // before we grab the screen.
            Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            var png = GdiCaptureEngine.CapturePng(px, py, pw, ph);
            var path = GdiCaptureEngine.SaveToDisk(png);

            CopyPngToClipboard(png);

            _toast(
                Strings.T("capture.toast.saved.title"),
                string.Format(Strings.T("capture.toast.saved.body"), $"{pw}×{ph}", Path.GetFileName(path)));
        }
        catch (Exception ex)
        {
            _toast(Strings.T("capture.error.title"), ex.Message);
        }
        finally
        {
            _capturing = false;
        }
    }

    private static void CopyPngToClipboard(byte[] pngBytes)
    {
        try
        {
            using var ms = new MemoryStream(pngBytes);
            var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];
            frame.Freeze();
            Clipboard.SetImage(frame);
        }
        catch
        {
            // Clipboard is finicky; failure is non-fatal.
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _hotkeys.Dispose();
        _disposed = true;
    }
}
