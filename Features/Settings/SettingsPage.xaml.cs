// Features/Settings/SettingsPage.xaml.cs
using ReturnToMonkee.Features.Onboarding;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee.Features.Settings;

public partial class SettingsPage : ContentPage
{
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IReminderService reminderService;
    private bool isLoadingSettings;

    public SettingsPage(IUserSettingsRepository userSettingsRepository, IReminderService reminderService)
    {
        InitializeComponent();
        this.userSettingsRepository = userSettingsRepository;
        this.reminderService = reminderService;
    }

    // Lädt bei jedem Besuch neu — zeigt nach Speichern auf Step2 sofort den neuen Wert.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var settings = await userSettingsRepository.GetAsync();
        SleepTimeLabel.Text = settings.SleepTime.ToString(@"hh\:mm");

        // Verhindert, dass das Setzen von IsChecked beim Laden den CheckedChanged-Handler auslöst
        // und den geladenen Wert sofort wieder (unnötig) zurückschreibt.
        isLoadingSettings = true;
        ShowOnboardingOnStartupCheckBox.IsChecked = settings.ShowOnboardingOnStartup;
        isLoadingSettings = false;
    }

    private async void OnEditSleepTimeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OnboardingStep2Page));
    }

    private async void OnTestSleepReminderClicked(object sender, EventArgs e)
    {
        await reminderService.TriggerSleepReminderAsync();
    }

    private async void OnShowOnboardingOnStartupCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (isLoadingSettings)
        {
            return;
        }

        var settings = await userSettingsRepository.GetAsync();
        settings.ShowOnboardingOnStartup = e.Value;
        await userSettingsRepository.SaveAsync(settings);
    }
}
