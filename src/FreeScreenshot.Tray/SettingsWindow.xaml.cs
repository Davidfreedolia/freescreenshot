using System.Diagnostics;
using System.IO;
using System.Windows;
using FreeScreenshot.Core.Capture;
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

        // Output
        OutH.Text = Strings.T("settings.output.heading");
        OutFolderLbl.Text = Strings.T("settings.output.folder");
        OutBrowseBtn.Content = Strings.T("settings.output.browse");
        OutOpenBtn.Content = Strings.T("settings.output.open");
        var folderPath = !string.IsNullOrWhiteSpace(_config.CaptureFolder)
            ? _config.CaptureFolder!
            : GdiCaptureEngine.DefaultSaveFolder;
        CaptureFolderText.Text = folderPath;
        CaptureFolderText.ToolTip = folderPath; // full path on hover for truncated cases

        OutFormatLbl.Text = Strings.T("settings.output.format");
        FmtPng.Content = Strings.T("settings.output.format.png");
        FmtJpg.Content = Strings.T("settings.output.format.jpg");
        var current = string.IsNullOrWhiteSpace(_config.CaptureFormat) ? "png" : _config.CaptureFormat.ToLowerInvariant();
        OutFormatCombo.SelectedItem = current == "jpg" ? FmtJpg : FmtPng;

        // Capture options
        CapH.Text = Strings.T("settings.capture.heading");
        EditorTitle.Text = Strings.T("settings.capture.editor.title");
        EditorDesc.Text  = Strings.T("settings.capture.editor.desc");
        EditorToggle.IsChecked = _config.AutoOpenEditor;
        SoundTitle.Text = Strings.T("settings.capture.sound.title");
        SoundDesc.Text  = Strings.T("settings.capture.sound.desc");
        SoundToggle.IsChecked = _config.PlaySound;
        PreviewTitle.Text = Strings.T("settings.capture.preview.title");
        PreviewDesc.Text  = Strings.T("settings.capture.preview.desc");
        PreviewToggle.IsChecked = _config.ShowPreview;

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
        ReplayOnboardingBtn.Content = Strings.T("settings.about.replay_onboarding");

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

    private void OnFormatChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        var item = OutFormatCombo.SelectedItem as ComboBoxItem;
        var fmt = (item?.Tag as string) ?? "png";
        _config.CaptureFormat = fmt;
        _config.Save();
    }

    private void OnReplayOnboarding(object sender, RoutedEventArgs e)
    {
        var existing = Application.Current.Windows.OfType<OnboardingWindow>().FirstOrDefault();
        if (existing is not null) { existing.Activate(); return; }
        new OnboardingWindow().Show();
    }

    private void OnBrowseCaptureFolder(object sender, RoutedEventArgs e)
    {
        var dlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = Strings.T("settings.output.folder"),
            SelectedPath = !string.IsNullOrWhiteSpace(_config.CaptureFolder)
                ? _config.CaptureFolder!
                : GdiCaptureEngine.DefaultSaveFolder,
            UseDescriptionForTitle = true,
        };
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _config.CaptureFolder = dlg.SelectedPath;
            _config.Save();
            CaptureFolderText.Text = dlg.SelectedPath;
            CaptureFolderText.ToolTip = dlg.SelectedPath;
        }
    }

    private void OnEditorChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _config.AutoOpenEditor = EditorToggle.IsChecked == true;
        _config.Save();
    }

    private void OnSoundChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _config.PlaySound = SoundToggle.IsChecked == true;
        _config.Save();
    }

    private void OnPreviewChanged(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        _config.ShowPreview = PreviewToggle.IsChecked == true;
        _config.Save();
    }

    private void OnOpenCaptureFolder(object sender, RoutedEventArgs e)
    {
        var folder = !string.IsNullOrWhiteSpace(_config.CaptureFolder)
            ? _config.CaptureFolder!
            : GdiCaptureEngine.DefaultSaveFolder;
        try
        {
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }
        catch { }
    }
}
