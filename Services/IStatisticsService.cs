using ReturnToMonkee.Models;

namespace ReturnToMonkee.Services;

public interface IStatisticsService
{
    /// <summary>
    /// Lädt die Statistiken für einen bestimmten Tag durch Aggregation von Events.
    /// </summary>
    Task<DailyStatistics> GetDailyStatisticsAsync(
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lädt die letzten 7 Tage Trend-Daten.
    /// </summary>
    Task<IReadOnlyList<DailyStatistics>> GetSevenDayTrendAsync(
        CancellationToken cancellationToken = default);
}