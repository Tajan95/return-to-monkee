using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Models;

namespace ReturnToMonkee.Services;

public class StatisticsService : IStatisticsService
{
    private readonly INotificationEventQueryRepository notificationEventQueryRepository;

    public StatisticsService(INotificationEventQueryRepository notificationEventQueryRepository)
    {
        this.notificationEventQueryRepository = notificationEventQueryRepository;
    }

    public async Task<DailyStatistics> GetDailyStatisticsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var events = await notificationEventQueryRepository.GetEventsForDateRangeAsync(
            startOfDay,
            endOfDay,
            cancellationToken);

        return AggregateEvents(startOfDay, events);
    }

    public async Task<IReadOnlyList<DailyStatistics>> GetSevenDayTrendAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var sevenDaysAgo = today.AddDays(-6);

        var events = await notificationEventQueryRepository.GetEventsForDateRangeAsync(
            sevenDaysAgo,
            today,
            cancellationToken);

        var result = new List<DailyStatistics>();

        for (int i = 0; i < 7; i++)
        {
            var date = sevenDaysAgo.AddDays(i);
            var dayEvents = events
                .Where(e => e.Time.Date == date)
                .ToList();

            result.Add(AggregateEvents(date, dayEvents));
        }

        return result.AsReadOnly();
    }

    private static DailyStatistics AggregateEvents(DateTime date, IReadOnlyList<NotificationEvent> events)
    {
        var stats = new DailyStatistics { Date = date };

        foreach (var @event in events)
        {
            if (string.IsNullOrEmpty(@event.AppReference))
                continue;

            if (@event.AppReference.Contains("movement-reminder/confirmed"))
                stats.MovementRemindersConfirmed++;
            else if (@event.AppReference.Contains("movement-reminder/ignored"))
                stats.MovementRemindersIgnored++;
            else if (@event.AppReference.Contains("sleep-reminder/confirmed"))
                stats.SleepRemindersConfirmed++;
            else if (@event.AppReference.Contains("sleep-reminder/ignored"))
                stats.SleepRemindersIgnored++;
            else if (@event.AppReference.Contains("time-limit") &&
                     @event.AppReference.Contains("exceeded"))
                stats.LimitsExceeded++;
        }

        return stats;
    }
}