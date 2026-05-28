using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Localization;
using Application = System.Windows.Application;

namespace FreeScreenshot;

internal sealed class TrayHost : IDisposable
{
    private readonly App _app;
    private readonly NotifyIcon _icon;
    private string? _lastToastPath;
    private bool _disposed;

    public TrayHost(App app)
    {
        _app = app;

        _icon = new NotifyIcon
        {
            Icon = LoadIcon(),
            Text = Strings.T("tray.tooltip"),
            Visible = true,
            ContextMenuStrip = BuildMenu(),
        };
        _icon.MouseDoubleClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) OpenSettings();
        };
        _icon.BalloonTipClicked += (_, _) =>
        {
            // Clicking a "capture saved" toast opens the file.
            if (!string.IsNullOrEmpty(_lastToastPath) && File.Exists(_lastToastPath))
            {
                TryOpen(_lastToastPath);
            }
        };
    }

    public void ShowStartupBalloon()
    {
        try
        {
            _icon.BalloonTipTitle = Strings.T("tray.balloon.title");
            _icon.BalloonTipText  = Strings.T("tray.balloon.message");
            _icon.BalloonTipIcon  = ToolTipIcon.Info;
            _lastToastPath = null;
            _icon.ShowBalloonTip(5000);
        }
        catch { }
    }

    /// <summary>Show a toast. If filePath is set, clicking the toast opens that file.</summary>
    public void ShowToast(string title, string body, string? filePath = null, ToolTipIcon icon = ToolTipIcon.Info)
    {
        try
        {
            _lastToastPath = filePath;
            _icon.BalloonTipTitle = title;
            _icon.BalloonTipText  = body;
            _icon.BalloonTipIcon  = icon;
            _icon.ShowBalloonTip(4000);
        }
        catch { }
    }

    /// <summary>Rebuild the context menu (call after AppConfig changes / new history item).</summary>
    public void RefreshMenu()
    {
        var existing = _icon.ContextMenuStrip;
        _icon.ContextMenuStrip = BuildMenu();
        existing?.Dispose();
    }

    private static Icon LoadIcon()
    {
        var exePath = Process.GetCurrentProcess().MainModule?.FileName
                      ?? Assembly.GetExecutingAssembly().Location;
        var icon = Icon.ExtractAssociatedIcon(exePath);
        return icon ?? SystemIcons.Application;
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        // Recent captures submenu.
        var history = _app.Config.RecentCaptures;
        var historyMenu = new ToolStripMenuItem(Strings.T("history.menu_header"));
        if (history.Count == 0)
        {
            var empty = new ToolStripMenuItem(Strings.T("history.empty")) { Enabled = false };
            historyMenu.DropDownItems.Add(empty);
        }
        else
        {
            foreach (var path in history.Take(8))
            {
                var label = Path.GetFileName(path);
                var item = new ToolStripMenuItem(label);
                var captured = path; // closure
                // Open in OUR editor — never hand off to Paint / the OS viewer.
                item.Click += (_, _) => Capture.EditorWindow.OpenForFile(captured);
                if (!File.Exists(path)) item.Enabled = false;
                historyMenu.DropDownItems.Add(item);
            }
            historyMenu.DropDownItems.Add(new ToolStripSeparator());
            var openFolder = new ToolStripMenuItem(Strings.T("history.open_folder"));
            openFolder.Click += (_, _) =>
            {
                var folder = !string.IsNullOrWhiteSpace(_app.Config.CaptureFolder)
                    ? _app.Config.CaptureFolder!
                    : Core.Capture.GdiCaptureEngine.DefaultSaveFolder;
                Directory.CreateDirectory(folder);
                TryOpen(folder);
            };
            historyMenu.DropDownItems.Add(openFolder);
        }
        menu.Items.Add(historyMenu);
        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add(Strings.T("menu.settings"), null, (_, _) => OpenSettings());
        menu.Items.Add(Strings.T("menu.about"),    null, (_, _) => OpenAbout());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Strings.T("menu.donate"),   null, (_, _) => OpenDonation());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Strings.T("menu.quit"),     null, (_, _) => _app.Shutdown());
        return menu;
    }

    private static void OpenSettings()
    {
        var existing = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
        if (existing is not null) { existing.Activate(); return; }
        new SettingsWindow().Show();
    }

    private static void OpenAbout()
    {
        var existing = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        if (existing is not null) { existing.Activate(); return; }
        new MainWindow().Show();
    }

    private static void OpenDonation() => TryOpen(Strings.DonationUrl);

    private static void TryOpen(string pathOrUrl)
    {
        try { Process.Start(new ProcessStartInfo(pathOrUrl) { UseShellExecute = true }); }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _icon.Visible = false;
        _icon.Dispose();
        _disposed = true;
    }
}
