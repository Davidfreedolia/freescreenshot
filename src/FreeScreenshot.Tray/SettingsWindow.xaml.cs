using System.Diagnostics;
using System.IO;
using System.Windows;
using FreeScreenshot.Core.Config;
using FreeScreenshot.Core.Localization;
using Application = System.Windows.Application;
using ComboBox = System.Windows.Controls.ComboBox;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

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

        Title              = Strings.T("settings.title");
        SidebarSubtitle.Text = Strings.T("settings.title");
        NavGeneral.Content = Strings.T("settings.nav.general");
        NavPrivacy.Content = Strings.T("settings.nav.privacy");
        NavAbout.Content   = Strings.T("settings.nav.about");

        // General
        GenH.Text = Strings.T("settings.general.heading");
        GenSub.Text = Strings.T("settings.general.sub");
        GenLangLbl.Text = Strings.T("settings.general.lang");
        GenPlaceholder.Text = Strings.T("settings.general.placeholder");
        foreach (ComboBoxItem item in LangCombo.Items)
        {
            if ((item.Tag as string) == Strings.Current)
            {
                LangCombo.SelectedItem = item;
                break;
            }
        }

        // Privacy
        PrivH.Text = Strings.T("settings.privacy.heading");
        PrivSub.Text = Strings.T("settings.privacy.sub");
        PrivToggleTitle.Text = Strings.T("settings.privacy.toggle.title");
        PrivToggleDesc.Text = Strings.T("settings.privacy.toggle.desc");
        PrivIdLabel.Text = Strings.T("settings.privacy.install_id_label");
        PrivOpenPolicy.Content = Strings.T("settings.privacy.open_policy");
        TrackingToggle.IsChecked = _config.TrackingOptedIn;
        InstallIdText.Text = _config.InstallId ?? "(pendent)";

        // About + Donate
        AboutH.Text = Strings.T("settings.about.heading");
        AboutSub.Text = Strings.T("settings.about.sub");
        AbVerLbl.Text = Strings.T("settings.about.version");
        AbLicLbl.Text = Strings.T("settings.about.license");
        AbCodeLbl.Text = Strings.T("settings.about.code");
        VersionText.Text = App.AppVersion;
        DonateAboutTitle.Text = Strings.T("settings.about.donate");
        DonateAboutSub.Text = Strings.T("settings.about.sub");
        DonateBtn.Content = Strings.T("settings.about.donate.btn");

        _loading = false;
    }

    private void OnNavChanged(object sender, RoutedEventArgs e)
    {
        if (PageGeneral is null) return;
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

    private void OnLangChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        var item = LangCombo.SelectedItem as ComboBoxItem;
        var lang = item?.Tag as string;
        if (string.IsNullOrEmpty(lang) || lang == Strings.Current) return;
        Strings.SetLang(lang);
        _config.Lang = lang;
        _config.Save();
        // Re-render this window.
        OnLoaded(this, new RoutedEventArgs());
    }

    private void OnOpenPrivacy(object sender, RoutedEventArgs e)
    {
        var local = Path.Combine(AppContext.BaseDirectory, "PRIVADESA.md");
        var url = File.Exists(local) ? local : Strings.SupportUrl + "/privacy";
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
    }

    private void OnDonate(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(Strings.DonationUrl) { UseShellExecute = true }); } catch { }
    }
}
