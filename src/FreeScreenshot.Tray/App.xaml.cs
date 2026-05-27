using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using FreeScreenshot.Capture;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Localization;
using FreeScreenshot.Core.Telemetry;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace FreeScreenshot;

public partial class App : Application
{
    private const string SingleInstanceMutexName = @"Global\Freedolia.FreeScreenshot.Singleton";

    private Mutex? _instanceMutex;
    internal TrayHost? _tray;
    private CaptureManager? _capture;
    public AppConfig Config { get; private set; } = new();
    public TelemetryClient? Telemetry { get; private set; }

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

        // Catch anything that would otherwise silently kill the app.
        AppDomain.CurrentDomain.UnhandledException += (_, ex) => LogFatal(ex.ExceptionObject);
        DispatcherUnhandledException += (_, ex) =>
        {
            LogFatal(ex.Exception);
            ex.Handled = true;
        };

        // Load config + language first.
        Config = AppConfig.Load();
        Config.EnsureInstallId();
        if (!string.IsNullOrWhiteSpace(Config.Lang)) Strings.SetLang(Config.Lang!);
        else
        {
            Strings.InitFromSystem();
            Config.Lang = Strings.Current;
            Config.Save();
        }

        // Special mode: uninstall feedback dialog only.
        if (e.Args.Any(a => a.Equals("--uninstall-feedback", StringComparison.OrdinalIgnoreCase)))
        {
            Telemetry = new TelemetryClient(Config);
            var dlg = new UninstallFeedbackWindow(Telemetry, AppVersion);
            dlg.ShowDialog();
            Shutdown();
            return;
        }

        // Single-instance gate (skip for --uninstall-feedback above).
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

        Telemetry = new TelemetryClient(Config);

        _tray = new TrayHost(this);

        // First-launch balloon so the user finds the icon.
        if (string.IsNullOrWhiteSpace(Config.ConsentedPrivacyVersion))
        {
            _tray.ShowStartupBalloon();
            Config.ConsentedPrivacyVersion = "v1";
            Config.Save();
        }

        if (e.Args.Any(a => a.Equals("--settings", StringComparison.OrdinalIgnoreCase)))
            new SettingsWindow().Show();

        // Wire up the capture hotkey AFTER the tray exists (so toasts can use it).
        _capture = new CaptureManager((title, body) =>
            _tray?.ShowToast(title, body));

        _ = Task.Run(BackgroundStartupTasksAsync);
    }

    private async Task BackgroundStartupTasksAsync()
    {
        if (Telemetry is null) return;

        var lang = Strings.Current;
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

    private static void LogFatal(object? ex)
    {
        try
        {
            var dir = AppConfig.ConfigDirectory;
            Directory.CreateDirectory(dir);
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n";
            File.AppendAllText(Path.Combine(dir, "fatal.log"), line);
        }
        catch
        {
            // last-resort logging — don't recurse on failure
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _capture?.Dispose();
        _tray?.Dispose();
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
