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

    private async void RunCaptureFlow(CaptureMode mode)
    {
        try
        {
            (int X, int Y, int W, int H) phys;
            Rect? selectionDips = null;

            if (mode == CaptureMode.Area)
            {
                var overlay = new SelectionOverlay();
                var ok = overlay.ShowDialog();
                if (ok != true || overlay.PhysicalSelection is not { } p || p.W < 1 || p.H < 1)
                    return;
                phys = p;
                selectionDips = overlay.Selection;
            }
            else
            {
                var dummy = Application.Current.MainWindow ?? new Window();
                var dpi = VisualTreeHelper.GetDpi(dummy).PixelsPerDip;
                phys = (
                    (int)Math.Round(SystemParameters.VirtualScreenLeft * dpi),
                    (int)Math.Round(SystemParameters.VirtualScreenTop  * dpi),
                    (int)Math.Round(SystemParameters.VirtualScreenWidth  * dpi),
                    (int)Math.Round(SystemParameters.VirtualScreenHeight * dpi));
            }

            Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            System.Threading.Thread.Sleep(60);

            var png = GdiCaptureEngine.CapturePng(phys.X, phys.Y, phys.W, phys.H);

            if (_config.PlaySound)
            {
                try { SystemSounds.Asterisk.Play(); } catch { }
            }

            // The floating toolbar appears for area captures and lets the user pick the action.
            // Full-screen + AutoOpenEditor go straight into the editor; otherwise it's save+copy.
            FloatingToolbar.Action action;
            if (mode == CaptureMode.Area && selectionDips is { } sel)
            {
                var tb = new FloatingToolbar();
                tb.PositionNear(sel, SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop);
                tb.ShowDialog();
                action = tb.ChosenAction;
            }
            else
            {
                action = _config.AutoOpenEditor ? FloatingToolbar.Action.Editor : FloatingToolbar.Action.Copy;
                // Also save by default on fullscreen
                if (!_config.AutoOpenEditor) action = FloatingToolbar.Action.Save;
            }

            switch (action)
            {
                case FloatingToolbar.Action.Cancel:
                case FloatingToolbar.Action.None:
                    return;

                case FloatingToolbar.Action.Copy:
                    CopyPngToClipboard(png);
                    _toast(
                        Strings.T("capture.toast.saved.title"),
                        string.Format(Strings.T("capture.toast.saved.body"), $"{phys.W}×{phys.H}", "(portapapers)"),
                        null);
                    return;

                case FloatingToolbar.Action.Save:
                    CopyPngToClipboard(png);
                    var savedPath = SaveAndToast(png, phys.W, phys.H);
                    NotifyCaptureSaved(savedPath);
                    return;

                case FloatingToolbar.Action.Editor:
                    {
                        var ed = new EditorWindow(png);
                        var ok = ed.ShowDialog();
                        if (ok != true || ed.EditedPng is null) return;
                        png = ed.EditedPng;
                        if (ed.ShouldSave)
                        {
                            var p = SaveAndToast(png, phys.W, phys.H);
                            NotifyCaptureSaved(p);
                        }
                        else
                        {
                            _toast(Strings.T("capture.toast.saved.title"),
                                string.Format(Strings.T("capture.toast.saved.body"), $"{phys.W}×{phys.H}", "(portapapers)"),
                                null);
                        }
                        return;
                    }

                case FloatingToolbar.Action.Pin:
                    {
                        CopyPngToClipboard(png);
                        var pin = new PinnedWindow(png);
                        pin.Show();
                        var path = SaveAndToast(png, phys.W, phys.H);
                        NotifyCaptureSaved(path);
                        return;
                    }

                case FloatingToolbar.Action.Ocr:
                    {
                        var text = await OcrHelper.ExtractAsync(png);
                        if (string.IsNullOrEmpty(text))
                        {
                            _toast(Strings.T("capture.error.title"), Strings.T("ocr.empty"), null);
                            return;
                        }
                        try { Clipboard.SetText(text); } catch { }
                        _toast(
                            Strings.T("ocr.copied.title"),
                            string.Format(Strings.T("ocr.copied.body"), text.Length),
                            null);
                        return;
                    }
            }
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

    private string SaveAndToast(byte[] png, int w, int h)
    {
        var folder = !string.IsNullOrWhiteSpace(_config.CaptureFolder)
            ? _config.CaptureFolder!
            : GdiCaptureEngine.DefaultSaveFolder;
        var path = GdiCaptureEngine.SaveToDisk(png, folder);
        _toast(
            Strings.T("capture.toast.saved.title"),
            string.Format(Strings.T("capture.toast.saved.body"), $"{w}×{h}", Path.GetFileName(path)),
            path);
        return path;
    }

    private void NotifyCaptureSaved(string path)
    {
        _config.RecentCaptures.Remove(path);
        _config.RecentCaptures.Insert(0, path);
        while (_config.RecentCaptures.Count > 20) _config.RecentCaptures.RemoveAt(_config.RecentCaptures.Count - 1);
        _config.Save();
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
