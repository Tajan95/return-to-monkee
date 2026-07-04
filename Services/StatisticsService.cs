using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Models;

namespace ReturnToMonkee.Services;

public class StatisticsService : IStatisticsService
{
    private readonly INotificationEventQueryRepository notificationEventQueryRepository;
    private readonly ITimeLimitRuleRepository timeLimitRuleRepository;

    public StatisticsService(
        INotificationEventQueryRepository notificationEventQueryRepository,
        ITimeLimitRuleRepository timeLimitRuleRepository)
    {
        this.notificationEventQueryRepository = notificationEventQueryRepository;
        this.timeLimitRuleRepository = timeLimitRuleRepository;
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

        var activeRuleCount = await GetActiveRuleCountAsync(cancellationToken);

        var stats = AggregateEvents(startOfDay, events);
        ApplyLimitsKept(stats, activeRuleCount);
        return stats;
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

        var activeRuleCount = await GetActiveRuleCountAsync(cancellationToken);

        var result = new List<DailyStatistics>();

        // Neueste zuerst: heute oben, dann absteigend bis vor 6 Tagen.
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);
            var dayEvents = events
                .Where(e => e.Time.Date == date)
                .ToList();

            var stats = AggregateEvents(date, dayEvents);

            // Eingehaltene Limits nur fuer heute ableiten. Fuer vergangene Tage existiert
            // keine historische Regelbasis -> keine erfundenen "kept"-Werte; dort zaehlen
            // nur die tatsaechlich erfassten Ereignisse (Ueberschreitungen, Reminder).
            if (date == today)
            {
                ApplyLimitsKept(stats, activeRuleCount);
            }

            result.Add(stats);
        }

        return result.AsReadOnly();
    }

    // Anzahl aktuell aktiver Zeitlimit-Regeln als Tagesbasis fuer die eingehaltenen Limits.
    // Bewusste MVP-Vereinfachung: die Historie der Regelanzahl wird nicht getrackt.
    private async Task<int> GetActiveRuleCountAsync(CancellationToken cancellationToken)
    {
        var rules = await timeLimitRuleRepository.GetAllAsync(cancellationToken);
        return rules.Count(rule => rule.IsEnabled);
    }

    // "Eingehalten" wird abgeleitet: aktive Regeln minus an diesem Tag ueberschrittene
    // (min. 0). Ein ruhiger Tag = alle aktiven Limits eingehalten.
    private static void ApplyLimitsKept(DailyStatistics stats, int activeRuleCount)
    {
        stats.LimitsKept = Math.Max(0, activeRuleCount - stats.LimitsExceeded);
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
