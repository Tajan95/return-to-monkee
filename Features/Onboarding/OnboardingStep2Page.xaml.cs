// Features/Onboarding/OnboardingStep2Page.xaml.cs
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.Onboarding;

public partial class OnboardingStep2Page : ContentPage
{
    private readonly IUserSettingsRepository userSettingsRepository;

    public OnboardingStep2Page(IUserSettingsRepository userSettingsRepository)
    {
        InitializeComponent();
        this.userSettingsRepository = userSettingsRepository;
    }

    // Befüllt TimePicker mit dem gespeicherten Wert.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ConfirmationLabel.IsVisible = false;
        var settings = await userSettingsRepository.GetAsync();
        SleepTimePicker.Time = settings.SleepTime;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Verhindert Doppel-Speicherung/Doppel-Navigation bei schnellem Doppelklick.
        if (sender is Button button)
        {
            button.IsEnabled = false;
        }

        var settings = await userSettingsRepository.GetAsync();
        settings.SleepTime = SleepTimePicker.Time ?? settings.SleepTime;
        await userSettingsRepository.SaveAsync(settings);
        ConfirmationLabel.IsVisible = true;

        await Task.Delay(800);
        await Shell.Current.GoToAsync("//home");
    }
}
