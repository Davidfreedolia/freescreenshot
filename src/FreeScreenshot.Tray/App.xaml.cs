using System.Reflection;
using System.Threading;
using System.Windows;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Telemetry;

namespace FreeScreenshot;

public partial class App : Application
{
    private const string SingleInstanceMutexName = @"Global\Freedolia.FreeScreenshot.Singleton";

    private Mutex? _instanceMutex;
    internal TrayHost? _tray;
    public AppConfig Config { get; private set; } = new();
    public TelemetryClient? Telemetry { get; private set; }

    /// <summary>App version pulled from the assembly metadata.</summary>
    public static string AppVersion
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is null ? "0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ---- Single-instance gate ----
        _instanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show(
                "FreeScreenshot ja s'està executant a la safata.",
                "FreeScreenshot",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // ---- Config + telemetry ----
        Config = AppConfig.Load();
        Config.EnsureInstallId();
        Telemetry = new TelemetryClient(Config);

        // ---- Tray ----
        _tray = new TrayHost(this);
        // First-run balloon so the user can find the icon.
        if (string.IsNullOrWhiteSpace(Config.ConsentedPrivacyVersion))
        {
            _tray.ShowStartupBalloon();
            Config.ConsentedPrivacyVersion = "v1";
            Config.Save();
        }

        // ---- Optional CLI flag: --settings opens Settings on launch.
        // Useful for pinned shortcuts and for first-run UX.
        if (e.Args.Any(a => a.Equals("--settings", StringComparison.OrdinalIgnoreCase)))
        {
            new SettingsWindow().Show();
        }

        // ---- Fire-and-forget background tasks ----
        _ = Task.Run(BackgroundStartupTasksAsync);
    }

    private async Task BackgroundStartupTasksAsync()
    {
        if (Telemetry is null) return;

        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var os = System.Environment.OSVersion.VersionString;
        await Telemetry.TryReportInstallAsync(AppVersion, lang, os);

        var latest = await Telemetry.TryGetLatestAsync();
        if (latest is { ok: true, version: { } remoteVersion } &&
            IsNewer(remoteVersion, AppVersion) &&
            !string.Equals(Config.SkippedUpdateVersion, remoteVersion, StringComparison.OrdinalIgnoreCase))
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var dlg = new UpdateAvailableWindow(latest, Config);
                dlg.Show();
            });
        }
    }

    /// <summary>Naive semver-ish compare: "1.2.3" vs "1.2.4". Returns true if `a` &gt; `b`.</summary>
    private static bool IsNewer(string a, string b)
    {
        static int[] Parts(string s) => s
            .TrimStart('v', 'V')
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => int.TryParse(p, out var n) ? n : 0)
            .ToArray();

        var pa = Parts(a);
        var pb = Parts(b);
        var max = Math.Max(pa.Length, pb.Length);
        for (var i = 0; i < max; i++)
        {
            var va = i < pa.Length ? pa[i] : 0;
            var vb = i < pb.Length ? pb[i] : 0;
            if (va > vb) return true;
            if (va < vb) return false;
        }
        return false;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
