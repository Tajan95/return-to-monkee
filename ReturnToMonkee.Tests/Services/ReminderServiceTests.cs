using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee.Tests.Services;

public sealed class ReminderServiceTests
{
    private static ReminderService CreateService(
        RecordingNotificationAdapter adapter,
        RecordingNotificationEventRepository eventRepository,
        TimeSpan sleepTime,
        bool sleepReminderEnabled = true,
        bool movementReminderEnabled = true,
        int movementIntervalMinutes = 100_000,
        int sleepReminderLeadMinutes = 0)
    {
        return new ReminderService(
            new StubOnboardingRepository
            {
                MovementReminderEnabled = movementReminderEnabled,
                MovementReminderIntervalMinutes = movementIntervalMinutes
            },
            eventRepository,
            adapter,
            new StubUserSettingsRepository
            {
                SleepTime = sleepTime,
                SleepReminderEnabled = sleepReminderEnabled,
                SleepReminderLeadMinutes = sleepReminderLeadMinutes
            });
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsTrue_WhenCurrentTimeReachesSleepTime_AndNotYetTriggeredToday()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        var now = new DateTime(2026, 7, 2, 22, 0, 0);

        var result = ReminderService.IsSleepReminderDue(sleepTime, leadMinutes: 0, now, lastTriggeredDate: null, lastTriggeredReminderTime: null);

        Assert.True(result);
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsFalse_WhenCurrentTimeIsBeforeSleepTime()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        var now = new DateTime(2026, 7, 2, 21, 59, 0);

        var result = ReminderService.IsSleepReminderDue(sleepTime, leadMinutes: 0, now, lastTriggeredDate: null, lastTriggeredReminderTime: null);

        Assert.False(result);
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsFalse_WhenAlreadyTriggeredToday_ForTheSameSleepTime()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        var now = new DateTime(2026, 7, 2, 23, 0, 0);
        var today = DateOnly.FromDateTime(now);

        var result = ReminderService.IsSleepReminderDue(sleepTime, leadMinutes: 0, now, lastTriggeredDate: today, lastTriggeredReminderTime: sleepTime);

        Assert.False(result);
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsTrue_WhenNewDayStarted_EvenIfTriggeredYesterday()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        var yesterday = new DateOnly(2026, 7, 1);
        var now = new DateTime(2026, 7, 2, 22, 0, 0);

        var result = ReminderService.IsSleepReminderDue(sleepTime, leadMinutes: 0, now, lastTriggeredDate: yesterday, lastTriggeredReminderTime: sleepTime);

        Assert.True(result);
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsTrue_WhenAlreadyTriggeredToday_ButSleepTimeWasChangedToALaterReachedValue()
    {
        // Deckt den Fall ab, dass der Reminder beim App-Start sofort fuer eine bereits
        // verstrichene (alte) Schlafenszeit ausgeloest wurde, bevor der Nutzer die
        // Schlafenszeit auf einen spaeteren, ebenfalls schon erreichten Wert aendert.
        var oldSleepTime = new TimeSpan(19, 0, 0);
        var newSleepTime = new TimeSpan(20, 0, 0);
        var now = new DateTime(2026, 7, 2, 20, 10, 0);
        var today = DateOnly.FromDateTime(now);

        var result = ReminderService.IsSleepReminderDue(
            newSleepTime,
            leadMinutes: 0,
            now,
            lastTriggeredDate: today,
            lastTriggeredReminderTime: oldSleepTime);

        Assert.True(result);
    }

    [Fact]
    public void ComputeSleepReminderTime_SubtractsLeadFromSleepTime()
    {
        var reminderTime = ReminderService.ComputeSleepReminderTime(new TimeSpan(22, 0, 0), leadMinutes: 30);

        Assert.Equal(new TimeSpan(21, 30, 0), reminderTime);
    }

    [Fact]
    public void ComputeSleepReminderTime_WrapsAroundMidnight_WhenLeadExceedsSleepTime()
    {
        // Schlafenszeit 00:30, Vorlauf 60 Min -> 23:30 des Vortags.
        var reminderTime = ReminderService.ComputeSleepReminderTime(new TimeSpan(0, 30, 0), leadMinutes: 60);

        Assert.Equal(new TimeSpan(23, 30, 0), reminderTime);
    }

    [Fact]
    public void IsSleepReminderDue_FiresAtSleepTimeMinusLead_NotAtSleepTime()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        // Reminder mit 30 Min Vorlauf ist um 21:30 faellig.
        var atLeadTime = new DateTime(2026, 7, 2, 21, 30, 0);

        var result = ReminderService.IsSleepReminderDue(
            sleepTime, leadMinutes: 30, atLeadTime, lastTriggeredDate: null, lastTriggeredReminderTime: null);

        Assert.True(result);
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsFalse_BeforeLeadTime()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        // Eine Minute vor dem Vorlauf-Zeitpunkt (21:30) -> noch nicht faellig.
        var beforeLeadTime = new DateTime(2026, 7, 2, 21, 29, 0);

        var result = ReminderService.IsSleepReminderDue(
            sleepTime, leadMinutes: 30, beforeLeadTime, lastTriggeredDate: null, lastTriggeredReminderTime: null);

        Assert.False(result);
    }

    [Fact]
    public void IsSleepReminderDue_ReturnsFalse_AtSleepTime_WhenAlreadyFiredAtLeadTimeToday()
    {
        var sleepTime = new TimeSpan(22, 0, 0);
        var atSleepTime = new DateTime(2026, 7, 2, 22, 0, 0);
        var today = DateOnly.FromDateTime(atSleepTime);
        var leadTime = new TimeSpan(21, 30, 0);

        var result = ReminderService.IsSleepReminderDue(
            sleepTime, leadMinutes: 30, atSleepTime, lastTriggeredDate: today, lastTriggeredReminderTime: leadTime);

        Assert.False(result);
    }

    [Fact]
    public async Task CheckDueAsync_TriggersAtLeadTime_WhenLeadOffsetConfigured()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(
            adapter,
            eventRepository,
            sleepTime: new TimeSpan(22, 0, 0),
            sleepReminderLeadMinutes: 30);
        // 21:30 = 22:00 minus 30 Min Vorlauf.
        var atLeadTime = new DateTime(2026, 7, 2, 21, 30, 0);

        await service.CheckDueAsync(atLeadTime);

        var savedEvent = Assert.Single(
            eventRepository.SavedEvents,
            e => e.AppReference == "return-to-monkee://sleep-reminder/confirmed");
        Assert.NotNull(savedEvent);
    }

    [Fact]
    public async Task CheckDueAsync_DoesNotTriggerBeforeLeadTime_WhenLeadOffsetConfigured()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(
            adapter,
            eventRepository,
            sleepTime: new TimeSpan(22, 0, 0),
            sleepReminderLeadMinutes: 30);
        // 21:29 = eine Minute vor dem Vorlauf-Zeitpunkt.
        var beforeLeadTime = new DateTime(2026, 7, 2, 21, 29, 0);

        await service.CheckDueAsync(beforeLeadTime);

        Assert.Empty(adapter.PromptCalls);
        Assert.Empty(eventRepository.SavedEvents);
    }

    [Fact]
    public async Task CheckDueAsync_SendsPromptAndSavesConfirmedEvent_WhenSleepTimeReachedAndConfirmed()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));
        var now = new DateTime(2026, 7, 2, 22, 0, 0);

        await service.CheckDueAsync(now);

        var prompt = Assert.Single(adapter.PromptCalls);
        Assert.Equal("Schlafenszeit-Erinnerung", prompt.Title);

        var savedEvent = Assert.Single(
            eventRepository.SavedEvents,
            e => e.AppReference == "return-to-monkee://sleep-reminder/confirmed");
        Assert.Equal("Schlafenszeit bestätigt", savedEvent.Title);
    }

    [Fact]
    public async Task CheckDueAsync_SavesIgnoredEvent_WhenSleepTimeReachedAndDismissed()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: false);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));
        var now = new DateTime(2026, 7, 2, 22, 0, 0);

        await service.CheckDueAsync(now);

        var savedEvent = Assert.Single(
            eventRepository.SavedEvents,
            e => e.AppReference == "return-to-monkee://sleep-reminder/ignored");
        Assert.Equal("Schlafenszeit ignoriert", savedEvent.Title);
    }

    [Fact]
    public async Task CheckDueAsync_DoesNotTrigger_WhenCurrentTimeIsBeforeSleepTime()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));
        var now = new DateTime(2026, 7, 2, 21, 0, 0);

        await service.CheckDueAsync(now);

        Assert.Empty(adapter.PromptCalls);
        Assert.Empty(eventRepository.SavedEvents);
    }

    [Fact]
    public async Task CheckDueAsync_DoesNotTriggerSleepReminder_WhenSleepReminderIsDisabled()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(
            adapter,
            eventRepository,
            sleepTime: new TimeSpan(22, 0, 0),
            sleepReminderEnabled: false);
        var now = new DateTime(2026, 7, 2, 22, 0, 0);

        await service.CheckDueAsync(now);

        Assert.Empty(adapter.PromptCalls);
        Assert.Empty(eventRepository.SavedEvents);
    }

    [Fact]
    public async Task CheckDueAsync_DoesNotTriggerMovementReminder_WhenMovementReminderIsDisabled()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(
            adapter,
            eventRepository,
            sleepTime: new TimeSpan(23, 0, 0),
            sleepReminderEnabled: false,
            movementReminderEnabled: false,
            movementIntervalMinutes: 30);
        var now = DateTime.Now.AddMinutes(31);

        await service.CheckDueAsync(now);

        Assert.Empty(adapter.PromptCalls);
        Assert.Empty(eventRepository.SavedEvents);
    }

    [Fact]
    public async Task CheckDueAsync_TriggersOnlyOnce_WhenCalledRepeatedlyOnSameDay()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));
        var now = new DateTime(2026, 7, 2, 22, 0, 0);

        await service.CheckDueAsync(now);
        await service.CheckDueAsync(now.AddMinutes(1));
        await service.CheckDueAsync(now.AddHours(1));

        var sleepEvents = eventRepository.SavedEvents
            .Where(e => e.AppReference.StartsWith("return-to-monkee://sleep-reminder/"))
            .ToList();
        Assert.Single(sleepEvents);
    }

    [Fact]
    public async Task CheckDueAsync_TriggersAgain_OnNextDay()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));
        var day1 = new DateTime(2026, 7, 2, 22, 0, 0);
        var day2 = new DateTime(2026, 7, 3, 22, 0, 0);

        await service.CheckDueAsync(day1);
        await service.CheckDueAsync(day2);

        var sleepEvents = eventRepository.SavedEvents
            .Where(e => e.AppReference.StartsWith("return-to-monkee://sleep-reminder/"))
            .ToList();
        Assert.Equal(2, sleepEvents.Count);
    }

    [Fact]
    public async Task TriggerSleepReminderAsync_SendsPromptAndSavesEvent_RegardlessOfCurrentTime()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));

        await service.TriggerSleepReminderAsync();

        var savedEvent = Assert.Single(
            eventRepository.SavedEvents,
            e => e.AppReference == "return-to-monkee://sleep-reminder/confirmed");
        Assert.NotNull(savedEvent);
    }

    [Fact]
    public async Task TriggerSleepReminderAsync_DoesNotSuppress_TheSameDaysAutomaticReminder()
    {
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var service = CreateService(adapter, eventRepository, sleepTime: new TimeSpan(22, 0, 0));
        var beforeSleepTime = new DateTime(2026, 7, 2, 18, 0, 0);
        var atSleepTime = new DateTime(2026, 7, 2, 22, 0, 0);

        await service.TriggerSleepReminderAsync();
        await service.CheckDueAsync(beforeSleepTime);
        await service.CheckDueAsync(atSleepTime);

        var sleepEvents = eventRepository.SavedEvents
            .Where(e => e.AppReference.StartsWith("return-to-monkee://sleep-reminder/"))
            .ToList();
        Assert.Equal(2, sleepEvents.Count);
    }

    [Fact]
    public async Task CheckDueAsync_TriggersAgain_WhenSleepTimeIsChangedToALaterValueAfterAlreadyTriggeredToday()
    {
        // Reproduziert den App-Start-Fall: Schlafenszeit steht noch auf einem bereits
        // verstrichenen Wert (z.B. von gestern), der Reminder feuert deshalb sofort beim
        // ersten Check. Aendert der Nutzer die Schlafenszeit danach auf einen spaeteren,
        // aber ebenfalls schon erreichten Wert, muss der Reminder erneut feuern.
        var adapter = new RecordingNotificationAdapter(promptResult: true);
        var eventRepository = new RecordingNotificationEventRepository();
        var settingsRepository = new StubUserSettingsRepository { SleepTime = new TimeSpan(19, 0, 0) };
        var service = new ReminderService(
            new StubOnboardingRepository(),
            eventRepository,
            adapter,
            settingsRepository);
        var appStart = new DateTime(2026, 7, 2, 19, 10, 0);
        var afterSleepTimeChanged = new DateTime(2026, 7, 2, 20, 10, 0);

        await service.CheckDueAsync(appStart);

        settingsRepository.SleepTime = new TimeSpan(20, 0, 0);
        await service.CheckDueAsync(afterSleepTimeChanged);

        var sleepEvents = eventRepository.SavedEvents
            .Where(e => e.AppReference.StartsWith("return-to-monkee://sleep-reminder/"))
            .ToList();
        Assert.Equal(2, sleepEvents.Count);
    }

    private sealed class RecordingNotificationAdapter : INotificationAdapter
    {
        private readonly bool promptResult;

        public RecordingNotificationAdapter(bool promptResult)
        {
            this.promptResult = promptResult;
        }

        public List<(string Title, string Message, string ConfirmButton, string DismissButton)> PromptCalls { get; } = new();

        public Task SendAsync(string title, string message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> PromptAsync(
            string title,
            string message,
            string confirmButton,
            string dismissButton,
            CancellationToken cancellationToken = default)
        {
            PromptCalls.Add((title, message, confirmButton, dismissButton));
            return Task.FromResult(promptResult);
        }
    }

    private sealed class RecordingNotificationEventRepository : INotificationEventRepository
    {
        public List<NotificationEvent> SavedEvents { get; } = new();

        public Task SaveAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default)
        {
            SavedEvents.Add(notificationEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class StubOnboardingRepository : IOnboardingRepository
    {
        public bool MovementReminderEnabled { get; set; } = true;

        public int MovementReminderIntervalMinutes { get; set; } = 100_000;

        public Task<string?> GetGoalOrientationAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        // Large enough that it never fires within the offsets used by these tests,
        // keeping the movement-reminder check fully isolated from sleep-reminder assertions.
        public Task<int> GetMovementReminderIntervalMinutesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(MovementReminderIntervalMinutes);

        public Task<bool> GetMovementReminderEnabledAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(MovementReminderEnabled);

        public Task SaveGoalOrientationAsync(string goalOrientation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveMovementReminderIntervalMinutesAsync(int intervalMinutes, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveMovementReminderEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> IsOnboardingCompletedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class StubUserSettingsRepository : IUserSettingsRepository
    {
        public TimeSpan SleepTime { get; set; } = new(22, 0, 0);

        public bool SleepReminderEnabled { get; set; } = true;

        public int SleepReminderLeadMinutes { get; set; }

        public Task<UserSettingsEntity> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new UserSettingsEntity
            {
                SleepTime = SleepTime,
                SleepReminderEnabled = SleepReminderEnabled,
                SleepReminderLeadMinutes = SleepReminderLeadMinutes
            });

        public Task SaveAsync(UserSettingsEntity settings, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
