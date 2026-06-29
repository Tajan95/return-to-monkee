namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public sealed class NotificationEventRepository : INotificationEventRepository
{
    private readonly ILocalDatabase localDatabase;

    public NotificationEventRepository(ILocalDatabase localDatabase)
    {
        this.localDatabase = localDatabase;
    }

    public async Task SaveAsync(
        NotificationEvent notificationEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<NotificationEvent>();
        await connection.InsertAsync(notificationEvent);
    }
}
