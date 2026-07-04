using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Services;

namespace ReturnToMonkee.Tests.Services;

public sealed class StatisticsServiceTests
{
    [Fact]
    public async Task GetDailyStatisticsAsync_CountsTimeLimitExceeded_FromRulesAppReference()
    {
        var date = new DateTime(2026, 7, 4);

        // Exakt das Format, das RulesViewModel bei einer Ueberschreitung schreibt
        // (app://rules/time-limit/{id}/exceeded). Regressionstest fuer den frueheren
        // Mismatch gegen "time-limit-exceeded", der LimitsExceeded nie hochzaehlte.
        var events = new List<NotificationEvent>
        {
            Evt(date, $"app://rules/time-limit/{Guid.NewGuid()}/exceeded")
        };

        var service = new StatisticsService(new StubQueryRepository(events));

        var stats = await service.GetDailyStatisticsAsync(date);

        Assert.Equal(1, stats.LimitsExceeded);
    }

    [Fact]
    public async Task GetDailyStatisticsAsync_CountsMovementAndSleepReminders()
    {
        var date = new DateTime(2026, 7, 4);

        var events = new List<NotificationEvent>
        {
            Evt(date, "return-to-monkee://movement-reminder/confirmed"),
            Evt(date, "return-to-monkee://movement-reminder/ignored"),
            Evt(date, "return-to-monkee://sleep-reminder/confirmed"),
            Evt(date, "return-to-monkee://sleep-reminder/ignored")
        };

        var service = new StatisticsService(new StubQueryRepository(events));

        var stats = await service.GetDailyStatisticsAsync(date);

        Assert.Equal(1, stats.MovementRemindersConfirmed);
        Assert.Equal(1, stats.MovementRemindersIgnored);
        Assert.Equal(1, stats.SleepRemindersConfirmed);
        Assert.Equal(1, stats.SleepRemindersIgnored);
    }

    private static NotificationEvent Evt(DateTime date, string appReference) => new()
    {
        Id = Guid.NewGuid(),
        Time = date.AddHours(9),
        Title = "Test-Event",
        AppReference = appReference
    };

    private sealed class StubQueryRepository : INotificationEventQueryRepository
    {
        private readonly IReadOnlyList<NotificationEvent> events;

        public StubQueryRepository(IReadOnlyList<NotificationEvent> events) => this.events = events;

        public Task<IReadOnlyList<NotificationEvent>> GetEventsForDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
            => Task.FromResult(events);

        public Task<IReadOnlyList<NotificationEvent>> GetEventsByAppReferenceAsync(
            string appReferencePattern,
            CancellationToken cancellationToken = default)
            => Task.FromResult(events);
    }
}
