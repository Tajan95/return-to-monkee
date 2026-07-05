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
        SleepTime != loadedSleepTime;

    public async Task LoadAsync()
    {
        isLoading = true;
        try
        {
            var settings = await userSettingsRepository.GetAsync();
            loadedSleepTime = settings.SleepTime;
            SleepTime = settings.SleepTime;
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await settingsSaveLock.WaitAsync();
        try
        {
            var settings = await userSettingsRepository.GetAsync();
            settings.SleepTime = SleepTime;
            settings.SleepReminderEnabled = IsSleepReminderEnabled;
            await userSettingsRepository.SaveAsync(settings);
        }
        finally
        {
            settingsSaveLock.Release();
        }

        loadedSleepTime = SleepTime;
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
