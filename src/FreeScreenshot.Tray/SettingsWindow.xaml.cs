using System.Diagnostics;
using System.IO;
using System.Windows;
using FreeScreenshot.Core.Config;

namespace FreeScreenshot;

public partial class SettingsWindow : Window
{
    private readonly AppConfig _config;
    private bool _loading;

    public SettingsWindow()
    {
        InitializeComponent();

        _config = ((App)Application.Current).Config;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _loading = true;
        TrackingToggle.IsChecked = _config.TrackingOptedIn;
        InstallIdText.Text = _config.InstallId ?? "(pendent)";
        VersionText.Text = App.AppVersion;
        _loading = false;
    }

    private void OnNavChanged(object sender, RoutedEventArgs e)
    {
        if (PageGeneral is null) return; // event fires before InitializeComponent completes
        PageGeneral.Visibility = NavGeneral.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PagePrivacy.Visibility = NavPrivacy.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PageAbout.Visibility   = NavAbout.IsChecked   == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnTrackingChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _config.TrackingOptedIn = TrackingToggle.IsChecked == true;
        _config.Save();
    }

    private void OnOpenPrivacy(object sender, RoutedEventArgs e)
    {
        // Try to open the bundled PRIVADESA.md; otherwise the web policy.
        var local = Path.Combine(AppContext.BaseDirectory, "PRIVADESA.md");
        var url = File.Exists(local) ? local : "https://freedolia.com/freescreenshot/privacy";
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // best-effort
        }
    }
}
