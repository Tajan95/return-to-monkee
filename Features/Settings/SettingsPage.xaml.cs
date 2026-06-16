// Features/Settings/SettingsPage.xaml.cs
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.Settings;

public partial class SettingsPage : ContentPage
{
    private readonly IUserSettingsRepository userSettingsRepository;

    public SettingsPage(IUserSettingsRepository userSettingsRepository)
    {
        InitializeComponent();
        this.userSettingsRepository = userSettingsRepository;
    }

    // Lädt bei jedem Besuch neu — zeigt nach Speichern auf Step2 sofort den neuen Wert.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var settings = await userSettingsRepository.GetAsync();
        SleepTimeLabel.Text = settings.SleepTime.ToString(@"hh\:mm");
    }

    private async void OnEditSleepTimeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OnboardingStep2Page));
    }
}
