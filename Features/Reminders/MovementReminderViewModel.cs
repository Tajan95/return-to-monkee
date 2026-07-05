using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;
using System.Collections.ObjectModel;

namespace ReturnToMonkee.Features.Reminders;

/// <summary>
/// Bearbeiten-Page fuer den Bewegungs-Reminder: Intervall (30/60/90) aendern und
/// den Reminder zum Testen sofort ausloesen (schreibt ein Statistik-Event).
/// </summary>
public partial class MovementReminderViewModel : ObservableObject
{
    private readonly IOnboardingRepository onboardingRepository;
    private readonly IReminderService reminderService;

    // Zuletzt gespeicherter Wert — Grundlage fuer das Dirty-Tracking (Save nur aktiv,
    // wenn die Auswahl davon abweicht).
    private string loadedInterval = "60 Minuten";
    private bool isLoading;
    private int enabledSaveRequestVersion;
    private readonly SemaphoreSlim enabledSaveLock = new(1, 1);

    // Blendet die Bestaetigungsmeldung nach kurzer Zeit wieder aus; wird bei jeder neuen
    // Meldung/Aenderung abgebrochen, damit kein alter Timer eine neue Meldung loescht.
    private CancellationTokenSource? statusResetCts;

    public ObservableCollection<string> MovementReminderIntervals { get; } =
        new() { "30 Minuten", "60 Minuten", "90 Minuten" };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string selectedMovementReminderInterval = "60 Minuten";

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private bool isMovementReminderEnabled = true;

    public MovementReminderViewModel(
        IOnboardingRepository onboardingRepository,
        IReminderService reminderService)
    {
        this.onboardingRepository = onboardingRepository;
        this.reminderService = reminderService;
    }

    private bool CanSave =>
        IsMovementReminderEnabled &&
        SelectedMovementReminderInterval != loadedInterval;

    public async Task LoadAsync()
    {
        isLoading = true;
        try
        {
            var intervalMinutes =
                await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
            loadedInterval = $"{intervalMinutes} Minuten";
            var loadedIsMovementReminderEnabled =
                await onboardingRepository.GetMovementReminderEnabledAsync();
            SelectedMovementReminderInterval = loadedInterval;
            IsMovementReminderEnabled = loadedIsMovementReminderEnabled;
            StatusMessage = string.Empty;
        }
        finally
        {
            isLoading = false;
            SaveCommand.NotifyCanExecuteChanged();
            TestCommand.NotifyCanExecuteChanged();
        }
    }

    // Bei jeder Auswahl-Aenderung die "gespeichert"-Meldung zuruecksetzen.
    // Der Save-Button-Zustand wird ueber NotifyCanExecuteChangedFor aktualisiert.
    partial void OnSelectedMovementReminderIntervalChanged(string value)
    {
        statusResetCts?.Cancel();
        StatusMessage = string.Empty;
    }

    partial void OnIsMovementReminderEnabledChanged(bool value)
    {
        statusResetCts?.Cancel();
        StatusMessage = string.Empty;

        if (isLoading)
        {
            return;
        }

        SaveCommand.NotifyCanExecuteChanged();
        _ = SaveMovementReminderEnabledAsync(value, ++enabledSaveRequestVersion);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await onboardingRepository.SaveMovementReminderIntervalMinutesAsync(
            ParseIntervalMinutes(SelectedMovementReminderInterval));
        loadedInterval = SelectedMovementReminderInterval;
        SaveCommand.NotifyCanExecuteChanged();
        ShowTransientStatus("Bewegungs-Reminder gespeichert.");
    }

    private async Task SaveMovementReminderEnabledAsync(bool isEnabled, int requestVersion)
    {
        await enabledSaveLock.WaitAsync();
        try
        {
            if (requestVersion != enabledSaveRequestVersion)
            {
                return;
            }

            await onboardingRepository.SaveMovementReminderEnabledAsync(isEnabled);
        }
        finally
        {
            enabledSaveLock.Release();
        }

        if (requestVersion == enabledSaveRequestVersion)
        {
            SaveCommand.NotifyCanExecuteChanged();
            ShowTransientStatus(isEnabled
                ? "Bewegungs-Reminder aktiviert."
                : "Bewegungs-Reminder deaktiviert.");
        }
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

    // Testen nur bei aktivem Reminder — deaktiviert sonst auch den Button in der UI.
    [RelayCommand(CanExecute = nameof(IsMovementReminderEnabled))]
    private async Task TestAsync()
    {
        await reminderService.TriggerMovementReminderAsync();
    }

    private static int ParseIntervalMinutes(string value)
    {
        var digits = new string(value.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var minutes) ? minutes : 60;
    }
}
