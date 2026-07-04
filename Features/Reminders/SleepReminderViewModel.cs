using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee.Features.Reminders;

/// <summary>
/// Bearbeiten-Page fuer den Schlafenszeit-Reminder: Schlafenszeit setzen/aendern und
/// den Reminder zum Testen sofort ausloesen (schreibt ein Statistik-Event).
/// </summary>
public partial class SleepReminderViewModel : ObservableObject
{
    private readonly IUserSettingsRepository userSettingsRepository;
    private readonly IReminderService reminderService;

    private TimeSpan sleepTime = TimeSpan.FromHours(22);
    // Zuletzt gespeicherter Wert — Grundlage fuer das Dirty-Tracking.
    private TimeSpan loadedSleepTime = TimeSpan.FromHours(22);

    // Blendet die Bestaetigungsmeldung nach kurzer Zeit wieder aus; wird bei jeder neuen
    // Meldung/Aenderung abgebrochen, damit kein alter Timer eine neue Meldung loescht.
    private CancellationTokenSource? statusResetCts;

    [ObservableProperty]
    private string statusMessage = string.Empty;

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

    private bool CanSave => SleepTime != loadedSleepTime;

    public async Task LoadAsync()
    {
        var settings = await userSettingsRepository.GetAsync();
        loadedSleepTime = settings.SleepTime;
        SleepTime = settings.SleepTime;
        StatusMessage = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var settings = await userSettingsRepository.GetAsync();
        settings.SleepTime = SleepTime;
        await userSettingsRepository.SaveAsync(settings);
        loadedSleepTime = SleepTime;
        SaveCommand.NotifyCanExecuteChanged();
        ShowTransientStatus("Schlafenszeit gespeichert.");
    }

    [RelayCommand]
    private async Task TestAsync()
    {
        await reminderService.TriggerSleepReminderAsync();
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
