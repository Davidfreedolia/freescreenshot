using System.Diagnostics;
using System.Windows;
using FreeScreenshot.Core.Localization;

namespace FreeScreenshot;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "FreeScreenshot";
        TaglineText.Text = Strings.T("app.tagline");
        DonateHeader.Text = Strings.T("settings.about.donate");
        DonateSub.Text = Strings.T("settings.about.sub");
        DonateBtn.Content = Strings.T("settings.about.donate.btn");
        VersionFooter.Text = $"v{App.AppVersion} · GPLv3 · freedolia.com";
        CloseBtn.Content = Strings.T("common.close");
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnDonateClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Strings.DonationUrl) { UseShellExecute = true });
        }
        catch { /* best effort */ }
    }
}
