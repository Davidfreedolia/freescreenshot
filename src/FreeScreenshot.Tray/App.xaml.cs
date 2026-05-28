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
    private bool _mutexOwned;
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

        AppDomain.CurrentDomain.UnhandledException += (_, ex) => LogFatal(ex.ExceptionObject);
        DispatcherUnhandledException += (_, ex) =>
        {
            LogFatal(ex.Exception);
            ex.Handled = true;
        };

        // Apply dark title bar to every Window the app shows.
        FreeScreenshot.UI.DarkTitleBar.HookAll();

        Config = AppConfig.Load();
        Config.EnsureInstallId();
        if (!string.IsNullOrWhiteSpace(Config.Lang)) Strings.SetLang(Config.Lang!);
        else
        {
            Strings.InitFromSystem();
            Config.Lang = Strings.Current;
            Config.Save();
        }

        if (e.Args.Any(a => a.Equals("--uninstall-feedback", StringComparison.OrdinalIgnoreCase)))
        {
            Telemetry = new TelemetryClient(Config);
            var dlg = new UninstallFeedbackWindow(Telemetry, AppVersion);
            dlg.ShowDialog();
            Shutdown();
            return;
        }

        _instanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isNewInstance);
        _mutexOwned = isNewInstance;
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

        _capture = new CaptureManager(Config, (title, body, path) =>
        {
            _tray?.ShowToast(title, body, path);
            if (!string.IsNullOrEmpty(path)) _tray?.RefreshMenu();
        });

        if (e.Args.Any(a => a.Equals("--settings", StringComparison.OrdinalIgnoreCase)))
            new SettingsWindow().Show();

        // First-launch onboarding so users never lose track of the tray icon.
        // Mark it as done IMMEDIATELY when we decide to show it — closing
        // the window with the X must not re-trigger it on next boot.
        if (!Config.OnboardingDone)
        {
            Config.OnboardingDone = true;
            Config.ConsentedPrivacyVersion ??= "v1";
            Config.Save();
            var ob = new OnboardingWindow();
            ob.Show();
        }
        else
        {
            // Subsequent launches: nothing. The user has already met the app.
        }

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
            File.AppendAllText(Path.Combine(dir, "fatal.log"),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n");
        }
        catch { }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _capture?.Dispose();
        _tray?.Dispose();
        if (_mutexOwned)
        {
            try { _instanceMutex?.ReleaseMutex(); }
            catch (ApplicationException) { /* not owned anymore, fine */ }
        }
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
