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

        var service = new StatisticsService(
            new StubQueryRepository(events),
            new StubTimeLimitRuleRepository(enabledRuleCount: 0));

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

        var service = new StatisticsService(
            new StubQueryRepository(events),
            new StubTimeLimitRuleRepository(enabledRuleCount: 0));

        var stats = await service.GetDailyStatisticsAsync(date);

        Assert.Equal(1, stats.MovementRemindersConfirmed);
        Assert.Equal(1, stats.MovementRemindersIgnored);
        Assert.Equal(1, stats.SleepRemindersConfirmed);
        Assert.Equal(1, stats.SleepRemindersIgnored);
    }

    [Fact]
    public async Task GetDailyStatisticsAsync_AllActiveRulesKept_WhenNoExceedances()
    {
        var date = new DateTime(2026, 7, 4);

        var service = new StatisticsService(
            new StubQueryRepository(new List<NotificationEvent>()),
            new StubTimeLimitRuleRepository(enabledRuleCount: 3));

        var stats = await service.GetDailyStatisticsAsync(date);

        Assert.Equal(3, stats.LimitsKept);
        Assert.Equal(0, stats.LimitsExceeded);
        Assert.Equal(100, stats.LimitKeptRate);
    }

    [Fact]
    public async Task GetDailyStatisticsAsync_ExceededReducesKept_FromSameRuleBase()
    {
        var date = new DateTime(2026, 7, 4);

        var events = new List<NotificationEvent>
        {
            Evt(date, $"app://rules/time-limit/{Guid.NewGuid()}/exceeded")
        };

        var service = new StatisticsService(
            new StubQueryRepository(events),
            new StubTimeLimitRuleRepository(enabledRuleCount: 2));

        var stats = await service.GetDailyStatisticsAsync(date);

        Assert.Equal(1, stats.LimitsExceeded);
        Assert.Equal(1, stats.LimitsKept);
        Assert.Equal(50, stats.LimitKeptRate);
    }

    [Fact]
    public async Task GetDailyStatisticsAsync_LimitsKeptNeverNegative_WhenExceededMoreThanRules()
    {
        var date = new DateTime(2026, 7, 4);

        var events = new List<NotificationEvent>
        {
            Evt(date, $"app://rules/time-limit/{Guid.NewGuid()}/exceeded"),
            Evt(date, $"app://rules/time-limit/{Guid.NewGuid()}/exceeded")
        };

        var service = new StatisticsService(
            new StubQueryRepository(events),
            new StubTimeLimitRuleRepository(enabledRuleCount: 1));

        var stats = await service.GetDailyStatisticsAsync(date);

        Assert.Equal(2, stats.LimitsExceeded);
        Assert.Equal(0, stats.LimitsKept);
    }

    [Fact]
    public async Task GetSevenDayTrendAsync_TodayFirst_AndKeptOnlyForToday()
    {
        var service = new StatisticsService(
            new StubQueryRepository(new List<NotificationEvent>()),
            new StubTimeLimitRuleRepository(enabledRuleCount: 3));

        var trend = await service.GetSevenDayTrendAsync();

        Assert.Equal(7, trend.Count);
        // Neueste zuerst.
        Assert.Equal(DateTime.Today, trend[0].Date);
        Assert.Equal(DateTime.Today.AddDays(-6), trend[6].Date);
        // Heute: abgeleitete eingehaltene Limits.
        Assert.Equal(3, trend[0].LimitsKept);
        // Vergangene, nicht getrackte Tage: keine erfundenen "kept"-Werte.
        Assert.All(trend.Skip(1), day => Assert.Equal(0, day.LimitsKept));
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

    private sealed class StubTimeLimitRuleRepository : ITimeLimitRuleRepository
    {
        private readonly List<TimeLimitRule> rules;

        public StubTimeLimitRuleRepository(int enabledRuleCount)
        {
            rules = Enumerable.Range(0, enabledRuleCount)
                .Select(_ => new TimeLimitRule { Id = Guid.NewGuid(), IsEnabled = true })
                .ToList();
        }

        public Task SaveInitialTimeLimitRuleAsync(
            string category,
            int timeLimitMinutes,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<List<TimeLimitRule>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(rules);
    }
}
