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

        // Verhindert, dass das Setzen von IsToggled beim Laden den Toggled-Handler auslöst
        // und den geladenen Wert sofort wieder (unnötig) zurückschreibt.
        isLoadingSettings = true;
        ShowOnboardingOnStartupSwitch.IsToggled = settings.ShowOnboardingOnStartup;
        isLoadingSettings = false;

        await UpdateDatabaseStatusAsync();
        await UpdateSeedButtonStateAsync();
    }

    // Seed-Button deaktivieren, solange die Demo-Regeln schon vorhanden sind.
    private async Task UpdateSeedButtonStateAsync()
    {
        var alreadySeeded = await demoDataSeeder.AreDemoRulesPresentAsync();
        SeedRulesButton.IsEnabled = !alreadySeeded;

        // Seite ist ein Singleton -> Label bei jedem Besuch neu setzen, damit nach einem Reset
        // keine veraltete "bereits vorhanden"-Meldung stehen bleibt.
        if (alreadySeeded)
        {
            SeedStatusLabel.Text = "Demo-Regeln sind bereits vorhanden.";
            SeedStatusLabel.IsVisible = true;
        }
        else
        {
            SeedStatusLabel.Text = string.Empty;
            SeedStatusLabel.IsVisible = false;
        }
    }

    // Diagnose-Badge: Healthcheck der lokalen DB (grün = bereit, rot = nicht verfügbar).
    private async Task UpdateDatabaseStatusAsync()
    {
        var health = await localDatabase.CheckHealthAsync();

        DatabaseStatusLabel.Text = health.Message;
        DatabaseStatusBadge.BackgroundColor = health.IsReady
            ? GetColor("WarmSelected")
            : GetColor("WarmDanger");
    }

    private static Color GetColor(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.Gray;

    // Entwickler-Werkzeug: legt Demo-Zeitlimit-Regeln an (idempotent) und meldet das Ergebnis.
    private async void OnSeedRulesClicked(object sender, EventArgs e)
    {
        var created = await demoDataSeeder.SeedRulesAsync();

        SeedStatusLabel.Text = created > 0
            ? $"{created} Demo-Regel(n) angelegt."
            : "Demo-Regeln waren bereits vorhanden.";
        SeedStatusLabel.IsVisible = true;

        // Idempotent — nach dem Anlegen ist ein erneutes Seeden ein No-Op, Button deaktivieren.
        SeedRulesButton.IsEnabled = false;
    }

    private async void OnShowOnboardingOnStartupToggled(object sender, ToggledEventArgs e)
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
