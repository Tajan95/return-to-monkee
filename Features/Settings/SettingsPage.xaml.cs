// Features/Settings/SettingsPage.xaml.cs
using ReturnToMonkee.Infrastructure.Persistence;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee.Features.Settings;

public partial class SettingsPage : ContentPage
{
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IReminderService reminderService;
    private readonly ILocalDatabase localDatabase;
    private readonly DemoDataSeeder demoDataSeeder;
    private bool isLoadingSettings;

    public SettingsPage(
        IUserSettingsRepository userSettingsRepository,
        IReminderService reminderService,
        ILocalDatabase localDatabase,
        DemoDataSeeder demoDataSeeder)
    {
        InitializeComponent();
        this.userSettingsRepository = userSettingsRepository;
        this.reminderService = reminderService;
        this.localDatabase = localDatabase;
        this.demoDataSeeder = demoDataSeeder;
    }

    // Lädt bei jedem Besuch neu.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var settings = await userSettingsRepository.GetAsync();

        // Verhindert, dass das Setzen von IsChecked beim Laden den CheckedChanged-Handler auslöst
        // und den geladenen Wert sofort wieder (unnötig) zurückschreibt.
        isLoadingSettings = true;
        ShowOnboardingOnStartupCheckBox.IsChecked = settings.ShowOnboardingOnStartup;
        isLoadingSettings = false;

        await UpdateDatabaseStatusAsync();
    }

    // Diagnose-Anzeige (aus dem Dashboard hierher verschoben): stellt Seed-Daten sicher
    // und zeigt Healthcheck + Anzahl Seed-Datensätze.
    private async Task UpdateDatabaseStatusAsync()
    {
        var seedEntityCount = await demoDataSeeder.EnsureSeedDataAsync();
        var health = await localDatabase.CheckHealthAsync();

        DatabaseStatusLabel.Text =
            $"{health.Message} · {seedEntityCount} Seed-Datensätze verfügbar";
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

    // Werkseinstellungen: loescht alle Daten nach ausdruecklicher Bestaetigung und startet
    // die App mit dem Onboarding neu.
    private async void OnResetAllDataClicked(object sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            "Alle Daten zurücksetzen?",
            "Alle Ziele, Regeln, Einstellungen und Verlaufsdaten werden dauerhaft gelöscht. " +
            "Die App startet danach mit dem Onboarding neu. Dies kann nicht rückgängig gemacht werden.",
            "Zurücksetzen",
            "Abbrechen");

        if (!confirmed)
        {
            return;
        }

        await reminderService.StopAsync();
        await localDatabase.ResetAllDataAsync();
        await Shell.Current.GoToAsync("//onboarding");
    }
}
