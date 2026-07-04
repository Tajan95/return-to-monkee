using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Features.Interventions;

public sealed class TimeLimitInterventionViewModel : INotifyPropertyChanged
{
    public const string InterventionTitle = "Zeitlimit ueberschritten";
    public const string InterventionMessage = "Kurzer Check-in: Was brauchst du gerade, bevor du weitermachst?";
    public const string EventAppReference = "app://simulation/time-limit-exceeded";

    private readonly INotificationAdapter notificationAdapter;
    private readonly INotificationEventRepository notificationEventRepository;
    private bool isSoftInterventionVisible;
    private bool isBusy;
    private string statusMessage = "Bereit fuer eine simulierte Zeitlimit-Überschreitung.";

    public TimeLimitInterventionViewModel(
        INotificationAdapter notificationAdapter,
        INotificationEventRepository notificationEventRepository)
    {
        this.notificationAdapter = notificationAdapter;
        this.notificationEventRepository = notificationEventRepository;
    }

    public bool IsSoftInterventionVisible
    {
        get => isSoftInterventionVisible;
        private set => SetProperty(ref isSoftInterventionVisible, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public async Task MarkTimeLimitExceededAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            IsSoftInterventionVisible = true;
            StatusMessage = "Zeitlimit-Überschreitung markiert.";

            await notificationAdapter.SendAsync(
                InterventionTitle,
                InterventionMessage,
                cancellationToken);

            var notificationEvent = new NotificationEvent
            {
                Id = Guid.NewGuid(),
                Time = DateTimeOffset.UtcNow,
                Title = InterventionTitle,
                Message = InterventionMessage,
                AppReference = EventAppReference
            };

            await notificationEventRepository.SaveAsync(notificationEvent, cancellationToken);
            StatusMessage = "Zeitlimit-Überschreitung gespeichert.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
