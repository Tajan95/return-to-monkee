namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public interface INotificationEventRepository
{
    Task SaveAsync(
        NotificationEvent notificationEvent,
        CancellationToken cancellationToken = default);
}
