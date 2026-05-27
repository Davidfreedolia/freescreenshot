using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;
using H.NotifyIcon.Core;

namespace FreeScreenshot;

/// <summary>
/// Owns the tray icon and its context menu. The app lives here when no window is open.
/// </summary>
internal sealed class TrayHost : IDisposable
{
    private readonly App _app;
    private readonly TaskbarIcon _icon;
    private bool _disposed;

    public TrayHost(App app)
    {
        _app = app;

        _icon = new TaskbarIcon
        {
            ToolTipText = "FreeScreenshot",
            IconSource = IconFactory.CreateTrayIcon(),
            ContextMenu = BuildMenu(),
        };
        _icon.TrayMouseDoubleClick += (_, _) => OpenSettings();

        // Force the icon to materialise immediately — otherwise it may stay
        // un-rendered until the first system tray refresh.
        _icon.ForceCreate();
    }

    /// <summary>Shows a first-run balloon so the user knows where the app went.</summary>
    public void ShowStartupBalloon()
    {
        try
        {
            _icon.ShowNotification(
                title: "FreeScreenshot",
                message: "Visc a la safata. Clica dret sobre la icona per opcions.",
                icon: NotificationIcon.Info);
        }
        catch
        {
            // Older Windows versions or restricted notification policies — fail silently.
        }
    }

    private ContextMenu BuildMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(MakeItem("Configuració…", "Ctrl+,", OnSettingsClick));
        menu.Items.Add(MakeItem("Quant a",          null,    OnAboutClick));
        menu.Items.Add(new Separator());
        menu.Items.Add(MakeItem("Sortir",           null,    OnQuitClick));
        return menu;
    }

    private static MenuItem MakeItem(string header, string? gesture, RoutedEventHandler handler)
    {
        var item = new MenuItem { Header = header };
        if (!string.IsNullOrEmpty(gesture)) item.InputGestureText = gesture;
        item.Click += handler;
        return item;
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e) => OpenSettings();
    private void OnAboutClick(object sender, RoutedEventArgs e) => OpenAbout();

    private void OnQuitClick(object sender, RoutedEventArgs e) => _app.Shutdown();

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

    public void Dispose()
    {
        if (_disposed) return;
        _icon.Dispose();
        _disposed = true;
    }
}
