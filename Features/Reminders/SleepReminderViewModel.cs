using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Features.Reminders;

/// <summary>
/// Bearbeiten-Page fuer den Schlafenszeit-Reminder: Schlafenszeit setzen/aendern und
/// den Reminder zum Testen sofort ausloesen (schreibt ein Statistik-Event).
/// </summary>
public partial class SleepReminderViewModel : ObservableObject
{
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IReminderService reminderService;

    // Vorlauf-Optionen (Anzeige-Text) und ihre Minuten-Werte. Reihenfolge = Picker-Reihenfolge.
    private static readonly (string Label, int Minutes)[] LeadOptions =
    {
        ("Zur Schlafenszeit", 0),
        ("15 Minuten vorher", 15),
        ("30 Minuten vorher", 30),
        ("60 Minuten vorher", 60)
    };

    private TimeSpan sleepTime = TimeSpan.FromHours(22);
    // Zuletzt gespeicherter Wert — Grundlage fuer das Dirty-Tracking.
    private TimeSpan loadedSleepTime = TimeSpan.FromHours(22);
    private string loadedSleepReminderLead = LeadOptions[0].Label;
    private bool isLoading;
    private int enabledSaveRequestVersion;
    private readonly SemaphoreSlim settingsSaveLock = new(1, 1);

    // Blendet die Bestaetigungsmeldung nach kurzer Zeit wieder aus; wird bei jeder neuen
    // Meldung/Aenderung abgebrochen, damit kein alter Timer eine neue Meldung loescht.
    private CancellationTokenSource? statusResetCts;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private bool isSleepReminderEnabled = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string selectedSleepReminderLead = LeadOptions[0].Label;

    public ObservableCollection<string> SleepReminderLeadOptions { get; } =
        new(LeadOptions.Select(option => option.Label));

    public SleepReminderViewModel(
        IUserSettingsRepository userSettingsRepository,
        IReminderService reminderService)
    {
        this.userSettingsRepository = userSettingsRepository;
        this.reminderService = reminderService;
    }

    public TimeSpan SleepTime
    {
        get => sleepTime;
        set
        {
            if (SetProperty(ref sleepTime, value))
            {
                statusResetCts?.Cancel();
                StatusMessage = string.Empty;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private bool CanSave =>
        IsSleepReminderEnabled &&
        (SleepTime != loadedSleepTime ||
         SelectedSleepReminderLead != loadedSleepReminderLead);

    public async Task LoadAsync()
    {
        isLoading = true;
        try
        {
            var settings = await userSettingsRepository.GetAsync();
            loadedSleepTime = settings.SleepTime;
            loadedSleepReminderLead = FormatLead(settings.SleepReminderLeadMinutes);
            SleepTime = settings.SleepTime;
            SelectedSleepReminderLead = loadedSleepReminderLead;
            IsSleepReminderEnabled = settings.SleepReminderEnabled;
            StatusMessage = string.Empty;
        }
        finally
        {
            isLoading = false;
            SaveCommand.NotifyCanExecuteChanged();
            TestCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnIsSleepReminderEnabledChanged(bool value)
    {
        statusResetCts?.Cancel();
        StatusMessage = string.Empty;

        if (isLoading)
        {
            return;
        }

        SaveCommand.NotifyCanExecuteChanged();
        _ = SaveSleepReminderEnabledAsync(value, ++enabledSaveRequestVersion);
    }

    // Vorlauf gehoert zum Speichern-Button (kein Auto-Save wie der Aktiv-Schalter).
    partial void OnSelectedSleepReminderLeadChanged(string value)
    {
        statusResetCts?.Cancel();
        StatusMessage = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await settingsSaveLock.WaitAsync();
        try
        {
            var settings = await userSettingsRepository.GetAsync();
            settings.SleepTime = SleepTime;
            settings.SleepReminderLeadMinutes = ParseLeadMinutes(SelectedSleepReminderLead);
            settings.SleepReminderEnabled = IsSleepReminderEnabled;
            await userSettingsRepository.SaveAsync(settings);
        }
        finally
        {
            settingsSaveLock.Release();
        }

        loadedSleepTime = SleepTime;
        loadedSleepReminderLead = SelectedSleepReminderLead;
        SaveCommand.NotifyCanExecuteChanged();
        ShowTransientStatus("Schlafenszeit-Reminder gespeichert.");
    }

    private async Task SaveSleepReminderEnabledAsync(bool isEnabled, int requestVersion)
    {
        await settingsSaveLock.WaitAsync();
        try
        {
            if (requestVersion != enabledSaveRequestVersion)
            {
                return;
            }

            var settings = await userSettingsRepository.GetAsync();
            settings.SleepReminderEnabled = isEnabled;
            await userSettingsRepository.SaveAsync(settings);
        }
        finally
        {
            settingsSaveLock.Release();
        }

        if (requestVersion == enabledSaveRequestVersion)
        {
            SaveCommand.NotifyCanExecuteChanged();
            ShowTransientStatus(isEnabled
                ? "Schlafenszeit-Reminder aktiviert."
                : "Schlafenszeit-Reminder deaktiviert.");
        }
    }

    // Testen nur bei aktivem Reminder — deaktiviert sonst auch den Button in der UI.
    [RelayCommand(CanExecute = nameof(IsSleepReminderEnabled))]
    private async Task TestAsync()
    {
        await reminderService.TriggerSleepReminderAsync();
    }

    private static int ParseLeadMinutes(string label)
    {
        foreach (var option in LeadOptions)
        {
            if (option.Label == label)
                return option.Minutes;
        }

        return 0;
    }

    private static string FormatLead(int minutes)
    {
        foreach (var option in LeadOptions)
        {
            if (option.Minutes == minutes)
                return option.Label;
        }

        return LeadOptions[0].Label;
    }

    // Zeigt eine Meldung und blendet sie nach kurzer Zeit wieder aus.
    private async void ShowTransientStatus(string message)
    {
        statusResetCts?.Cancel();
        var cts = new CancellationTokenSource();
        statusResetCts = cts;

        StatusMessage = message;

        try
        {
            await Task.Delay(2500, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        StatusMessage = string.Empty;
    }
}
