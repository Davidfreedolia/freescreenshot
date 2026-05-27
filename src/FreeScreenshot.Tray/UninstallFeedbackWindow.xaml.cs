using System.Windows;
using FreeScreenshot.Core.Localization;
using FreeScreenshot.Core.Telemetry;
using CheckBox = System.Windows.Controls.CheckBox;

namespace FreeScreenshot;

public partial class UninstallFeedbackWindow : Window
{
    // Reason codes must match the enum on the backend.
    private static readonly (string Code, string Key)[] ReasonRows =
    {
        ("no_what_expected",  "uninstall.reason.no_what_expected"),
        ("missing_feature",   "uninstall.reason.missing_feature"),
        ("too_slow",          "uninstall.reason.too_slow"),
        ("found_alternative", "uninstall.reason.found_alternative"),
        ("not_using",         "uninstall.reason.not_using"),
        ("bugs",              "uninstall.reason.bugs"),
        ("temporary",         "uninstall.reason.temporary"),
        ("other",             "uninstall.reason.other"),
    };

    private readonly TelemetryClient _telemetry;
    private readonly string _appVersion;
    private readonly CheckBox[] _checks;

    public UninstallFeedbackWindow(TelemetryClient telemetry, string appVersion)
    {
        InitializeComponent();
        _telemetry = telemetry;
        _appVersion = appVersion;

        Title = Strings.T("uninstall.title");
        TitleText.Text = Strings.T("uninstall.title");
        SubText.Text = Strings.T("uninstall.sub");
        NoteBox.Tag = Strings.T("uninstall.note.placeholder");
        NoteBox.Text = "";
        SendBtn.Content = Strings.T("uninstall.send");
        SkipBtn.Content = Strings.T("uninstall.skip");

        _checks = new[] { R1, R2, R3, R4, R5, R6, R7, R8 };
        for (var i = 0; i < ReasonRows.Length && i < _checks.Length; i++)
        {
            _checks[i].Content = Strings.T(ReasonRows[i].Key);
            _checks[i].Tag = ReasonRows[i].Code;
        }
    }

    private async void OnSend(object sender, RoutedEventArgs e)
    {
        SendBtn.IsEnabled = false;
        SkipBtn.IsEnabled = false;
        var reasons = _checks
            .Where(c => c.IsChecked == true)
            .Select(c => c.Tag as string)
            .Where(s => !string.IsNullOrEmpty(s))
            .Cast<string>()
            .ToList();
        var note = NoteBox.Text?.Trim();
        try
        {
            await _telemetry.TryReportUninstallAsync(reasons, note, _appVersion, TimeSpan.FromSeconds(3));
        }
        catch
        {
            // Best effort — uninstall continues either way.
        }
        Close();
    }

    private void OnSkip(object sender, RoutedEventArgs e) => Close();
}
