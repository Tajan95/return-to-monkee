using ReturnToMonkee.Features.Interventions;
using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;

namespace ReturnToMonkee.Tests.Features.Interventions;

public sealed class TimeLimitInterventionViewModelTests
{
    [Fact]
    public async Task MarkTimeLimitExceededAsync_ShowsSoftInterventionAndStoresEvent()
    {
        var adapter = new RecordingNotificationAdapter();
        var repository = new RecordingNotificationEventRepository();
        var viewModel = new TimeLimitInterventionViewModel(adapter, repository);

        await viewModel.MarkTimeLimitExceededAsync();

        Assert.True(viewModel.IsSoftInterventionVisible);
        Assert.Equal("Zeitlimit-Überschreitung gespeichert.", viewModel.StatusMessage);
        Assert.Equal(TimeLimitInterventionViewModel.InterventionTitle, adapter.Title);
        Assert.Equal(TimeLimitInterventionViewModel.InterventionMessage, adapter.Message);

        var savedEvent = Assert.Single(repository.SavedEvents);
        Assert.Equal(TimeLimitInterventionViewModel.InterventionTitle, savedEvent.Title);
        Assert.Equal(TimeLimitInterventionViewModel.InterventionMessage, savedEvent.Message);
        Assert.Equal(TimeLimitInterventionViewModel.EventAppReference, savedEvent.AppReference);
        Assert.NotEqual(Guid.Empty, savedEvent.Id);
    }

    private sealed class RecordingNotificationAdapter : INotificationAdapter
    {
        public string? Title { get; private set; }
        public string? Message { get; private set; }

        public Task SendAsync(string title, string message, CancellationToken cancellationToken = default)
        {
            Title = title;
            Message = message;
            return Task.CompletedTask;
        }

        public Task<bool> PromptAsync(
            string title,
            string message,
            string confirmButton,
            string dismissButton,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class RecordingNotificationEventRepository : INotificationEventRepository
    {
        public List<NotificationEvent> SavedEvents { get; } = new();

        public Task SaveAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken = default)
        {
            SavedEvents.Add(notificationEvent);
            return Task.CompletedTask;
        }
    }
}
