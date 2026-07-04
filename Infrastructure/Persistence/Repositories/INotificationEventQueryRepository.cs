namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public interface INotificationEventQueryRepository
{
    /// <summary>
    /// Lädt alle Events innerhalb eines Zeitraums.
    /// </summary>
    Task<IReadOnlyList<NotificationEvent>> GetEventsForDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lädt Events basierend auf AppReference-Pattern (z.B. "movement-reminder/confirmed").
    /// </summary>
    Task<IReadOnlyList<NotificationEvent>> GetEventsByAppReferenceAsync(
        string appReferencePattern,
        CancellationToken cancellationToken = default);
}