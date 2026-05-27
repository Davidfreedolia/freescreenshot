using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeScreenshot.Core.Capture;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Localization;
using FreeScreenshot.Hotkeys;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;

namespace FreeScreenshot.Capture;

internal sealed class CaptureManager : IDisposable
{
    private const uint VK_1 = 0x31;
    private const uint VK_3 = 0x33;

    private readonly GlobalHotkey _hotkeys = new();
    private readonly AppConfig _config;
    private readonly Action<string, string, string?> _toast;
    private bool _capturing;
    private bool _disposed;

    public CaptureManager(AppConfig config, Action<string, string, string?> showToast)
    {
        _config = config;
        _toast = showToast;
        var mods = GlobalHotkey.Modifiers.Ctrl | GlobalHotkey.Modifiers.Shift;

        if (_hotkeys.Register(mods, VK_1, () => Trigger(CaptureMode.Area)) == 0)
        {
            _toast(Strings.T("capture.error.title"), Strings.T("capture.error.hotkey_busy"), null);
        }
        // Best-effort: Ctrl+Shift+3 for full-screen capture.
        _hotkeys.Register(mods, VK_3, () => Trigger(CaptureMode.FullScreen));
    }

    private enum CaptureMode { Area, FullScreen }

    private void Trigger(CaptureMode mode)
    {
        if (_capturing) return;
        _capturing = true;
        try
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => RunCaptureFlow(mode)));
        }
        catch
        {
            _capturing = false;
        }
    }

    private void RunCaptureFlow(CaptureMode mode)
    {
        try
        {
            (int X, int Y, int W, int H) phys;

            if (mode == CaptureMode.Area)
            {
                var overlay = new SelectionOverlay();
                var ok = overlay.ShowDialog();
                if (ok != true || overlay.PhysicalSelection is not { } p || p.W < 1 || p.H < 1)
                    return;
                phys = p;
            }
            else
            {
                var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window()).PixelsPerDip;
                phys = (
                    (int)Math.Round(SystemParameters.VirtualScreenLeft * dpi),
                    (int)Math.Round(SystemParameters.VirtualScreenTop  * dpi),
                    (int)Math.Round(SystemParameters.VirtualScreenWidth  * dpi),
                    (int)Math.Round(SystemParameters.VirtualScreenHeight * dpi));
            }

            // Let the overlay un-paint before we grab.
            Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            System.Threading.Thread.Sleep(60);

            var png = GdiCaptureEngine.CapturePng(phys.X, phys.Y, phys.W, phys.H);

            if (_config.PlaySound)
            {
                try { SystemSounds.Asterisk.Play(); } catch { }
            }

            // Editor flow.
            if (_config.AutoOpenEditor)
            {
                var editor = new EditorWindow(png);
                var ok = editor.ShowDialog();
                if (ok != true || editor.EditedPng is null)
                {
                    return;
                }
                png = editor.EditedPng;
                if (!editor.ShouldSave)
                {
                    // User chose Copy-only — clipboard set in editor, no toast needed.
                    _toast(Strings.T("capture.toast.saved.title"),
                           string.Format(Strings.T("capture.toast.saved.body"), $"{phys.W}×{phys.H}", "(portapapers)"),
                           null);
                    return;
                }
            }
            else
            {
                CopyPngToClipboard(png);
            }

            var folder = !string.IsNullOrWhiteSpace(_config.CaptureFolder)
                ? _config.CaptureFolder!
                : GdiCaptureEngine.DefaultSaveFolder;
            var path = GdiCaptureEngine.SaveToDisk(png, folder);

            _toast(
                Strings.T("capture.toast.saved.title"),
                string.Format(Strings.T("capture.toast.saved.body"), $"{phys.W}×{phys.H}", Path.GetFileName(path)),
                path);
        }
        catch (Exception ex)
        {
            _toast(Strings.T("capture.error.title"), ex.Message, null);
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
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _hotkeys.Dispose();
        _disposed = true;
    }
}
