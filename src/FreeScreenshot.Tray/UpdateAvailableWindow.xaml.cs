using System.Diagnostics;
using System.Windows;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Telemetry;

namespace FreeScreenshot;

public partial class UpdateAvailableWindow : Window
{
    private readonly TelemetryClient.LatestResponse _latest;
    private readonly AppConfig _config;

    public UpdateAvailableWindow(TelemetryClient.LatestResponse latest, AppConfig config)
    {
        InitializeComponent();
        _latest = latest;
        _config = config;

        HeadlineText.Text = $"Nova versió de FreeScreenshot v{latest.version}";
        NotesText.Text = string.IsNullOrWhiteSpace(latest.notes)
            ? "Encara no hi ha notes de versió per aquesta release."
            : latest.notes;
    }

    private void OnUpdateClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_latest.download_url))
        {
            try
            {
                Process.Start(new ProcessStartInfo(_latest.download_url) { UseShellExecute = true });
            }
            catch
            {
                // best-effort
            }
        }
        Close();
    }

    private void OnLaterClick(object sender, RoutedEventArgs e)
    {
        // Just dismiss — will reappear on next launch.
        Close();
    }

    private void OnSkipClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_latest.version))
        {
            _config.SkippedUpdateVersion = _latest.version;
            _config.Save();
        }
        Close();
    }
}
