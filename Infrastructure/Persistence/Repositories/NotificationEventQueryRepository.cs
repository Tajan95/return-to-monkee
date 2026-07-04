using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public sealed class NotificationEventQueryRepository : INotificationEventQueryRepository
{
    private readonly ILocalDatabase localDatabase;

    public NotificationEventQueryRepository(ILocalDatabase localDatabase)
    {
        this.localDatabase = localDatabase;
    }

    public async Task<IReadOnlyList<NotificationEvent>> GetEventsForDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<NotificationEvent>();

        var startOffset = new DateTimeOffset(startDate);
        var endOffset = new DateTimeOffset(endDate.AddDays(1));

        var events = await connection.Table<NotificationEvent>()
            .Where(e => e.Time >= startOffset && e.Time < endOffset)
            .ToListAsync();

        return events.AsReadOnly();
    }

    public async Task<IReadOnlyList<NotificationEvent>> GetEventsByAppReferenceAsync(
        string appReferencePattern,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<NotificationEvent>();

        var events = await connection.Table<NotificationEvent>()
            .Where(e => e.AppReference.Contains(appReferencePattern))
            .ToListAsync();

        return events.AsReadOnly();
    }
}