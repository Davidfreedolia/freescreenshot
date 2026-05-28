using System.Diagnostics;
using System.Windows;
using Freezshot.Core.Config;
using Freezshot.Core.Localization;
using Application = System.Windows.Application;

namespace Freezshot;

public partial class OnboardingWindow : Window
{
    private readonly AppConfig _config;

    public OnboardingWindow()
    {
        InitializeComponent();
        _config = ((App)Application.Current).Config;

        WelcomeTitle.Text = Strings.T("onboarding.title");
        WelcomeSub.Text   = Strings.T("onboarding.sub");
        Step1Title.Text   = Strings.T("onboarding.step1.title");
        Step1Body.Text    = Strings.T("onboarding.step1.body");
        Step2Title.Text   = Strings.T("onboarding.step2.title");
        Step2Body.Text    = Strings.T("onboarding.step2.body");
        Step3Title.Text   = Strings.T("onboarding.step3.title");
        Step3Body.Text    = Strings.T("onboarding.step3.body");
        DoneBtn.Content   = Strings.T("onboarding.done");
        SettingsBtn.Content = Strings.T("menu.settings");

        DonateFooterText.Text          = Strings.T("onboarding.donate.hint");
        OnboardingDonateBtn.Content    = Strings.T("onboarding.donate.cta");
    }

    private void OnOnboardingDonate(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Strings.DonationUrl) { UseShellExecute = true });
        }
        catch { /* never block UX on a failed Open */ }
    }

    private void OnDone(object sender, RoutedEventArgs e)
    {
        _config.OnboardingDone = true;
        _config.Save();
        Close();
    }

    private void OnSettings(object sender, RoutedEventArgs e)
    {
        _config.OnboardingDone = true;
        _config.Save();
        new SettingsWindow().Show();
        Close();
    }
}
