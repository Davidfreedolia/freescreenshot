using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using FreeScreenshot.Core.Localization;
using Application = System.Windows.Application;

namespace FreeScreenshot;

/// <summary>
/// Tray host using the classic System.Windows.Forms.NotifyIcon — bulletproof
/// since .NET Framework 1.0. The icon is extracted from the .exe itself
/// (we set ApplicationIcon in the csproj), so we never touch pack URIs.
/// </summary>
internal sealed class TrayHost : IDisposable
{
    private readonly App _app;
    private readonly NotifyIcon _icon;
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
    }

    public void ShowStartupBalloon()
    {
        try
        {
            _icon.BalloonTipTitle = Strings.T("tray.balloon.title");
            _icon.BalloonTipText  = Strings.T("tray.balloon.message");
            _icon.BalloonTipIcon  = ToolTipIcon.Info;
            _icon.ShowBalloonTip(5000);
        }
        catch
        {
            // Some Windows configurations disable toast notifications entirely. Fail silently.
        }
    }

    /// <summary>Generic toast for capture results, errors, etc.</summary>
    public void ShowToast(string title, string body, ToolTipIcon icon = ToolTipIcon.Info)
    {
        try
        {
            _icon.BalloonTipTitle = title;
            _icon.BalloonTipText  = body;
            _icon.BalloonTipIcon  = icon;
            _icon.ShowBalloonTip(4000);
        }
        catch { /* swallow */ }
    }

    private static Icon LoadIcon()
    {
        // The .ico is embedded in the .exe as ApplicationIcon — extract it from ourselves.
        var exePath = Process.GetCurrentProcess().MainModule?.FileName
                      ?? Assembly.GetExecutingAssembly().Location;
        var icon = Icon.ExtractAssociatedIcon(exePath);
        return icon ?? SystemIcons.Application;
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
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

    private static void OpenDonation()
    {
        try
        {
            Process.Start(new ProcessStartInfo(Strings.DonationUrl) { UseShellExecute = true });
        }
        catch { /* best effort */ }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _icon.Visible = false;
        _icon.Dispose();
        _disposed = true;
    }
}
