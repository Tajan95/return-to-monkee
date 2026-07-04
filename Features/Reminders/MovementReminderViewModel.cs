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

    public ObservableCollection<string> MovementReminderIntervals { get; } =
        new() { "30 Minuten", "60 Minuten", "90 Minuten" };

    [ObservableProperty]
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

    public async Task LoadAsync()
    {
        var intervalMinutes =
            await onboardingRepository.GetMovementReminderIntervalMinutesAsync();
        SelectedMovementReminderInterval = $"{intervalMinutes} Minuten";
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await onboardingRepository.SaveMovementReminderIntervalMinutesAsync(
            ParseIntervalMinutes(SelectedMovementReminderInterval));
        StatusMessage = "Intervall gespeichert.";
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
