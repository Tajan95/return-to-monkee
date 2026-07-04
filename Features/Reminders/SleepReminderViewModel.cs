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
        set => SetProperty(ref sleepTime, value);
    }

    public async Task LoadAsync()
    {
        var settings = await userSettingsRepository.GetAsync();
        SleepTime = settings.SleepTime;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var settings = await userSettingsRepository.GetAsync();
        settings.SleepTime = SleepTime;
        await userSettingsRepository.SaveAsync(settings);
        StatusMessage = "Schlafenszeit gespeichert.";
    }

    [RelayCommand]
    private async Task TestAsync()
    {
        await reminderService.TriggerSleepReminderAsync();
    }
}
