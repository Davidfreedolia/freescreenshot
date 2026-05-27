using System.Windows;

namespace FreeScreenshot;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // TODO: single-instance check, register global hotkeys, init tray icon.
    }
}
