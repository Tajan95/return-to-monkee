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

    public ObservableCollection<string> MovementReminderIntervals { get; } =
        new() { "30 Minuten", "60 Minuten", "90 Minuten" };

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string selectedMovementReminderInterval = "60 Minuten";

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public MovementReminderViewModel(
        IOnboardingRepository onboardingRepository,
        IReminderService reminderService)
    {
        this.onboardingRepository = onboardingRepository;
        this.reminderService = reminderService;
    }

    private bool CanSave => SelectedMovementReminderInterval != loadedInterval;

    public async Task LoadAsync()
    {
        var intervalMinutes =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
        loadedInterval = $"{intervalMinutes} Minuten";
        SelectedMovementReminderInterval = loadedInterval;
        StatusMessage = string.Empty;
    }

    // Bei jeder Auswahl-Aenderung die "gespeichert"-Meldung zuruecksetzen.
    // Der Save-Button-Zustand wird ueber NotifyCanExecuteChangedFor aktualisiert.
    partial void OnSelectedMovementReminderIntervalChanged(string value)
    {
        StatusMessage = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await onboardingRepository.SaveMovementReminderIntervalMinutesAsync(
            ParseIntervalMinutes(SelectedMovementReminderInterval));
        loadedInterval = SelectedMovementReminderInterval;
        StatusMessage = "Intervall gespeichert.";
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
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
