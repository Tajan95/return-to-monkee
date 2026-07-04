using ReturnToMonkee.Features.Rules;
using ReturnToMonkee.Infrastructure.Notifications;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Tests.Helpers;

namespace ReturnToMonkee.Tests.Features.Rules;

public sealed class RulesViewModelTests
{
    [Fact]
    public async Task TestTimeLimitRuleAsync_SendsRuleSpecificNotificationAndStoresEvent()
    {
        var database = new InMemoryLocalDatabase();
        var connection = await database.GetConnectionAsync();
        await connection.CreateTableAsync<TimeLimitRule>();

        var rule = new TimeLimitRule
        {
            Id = Guid.NewGuid(),
            Title = "Social Media begrenzen",
            Description = "Tägliches Zeitlimit für Social Media: 30 Minuten",
            IsEnabled = true,
            TimeLimitMinutes = 30,
            TargetApplication = "Social Media"
        };

        await connection.InsertAsync(rule);

        var adapter = new RecordingNotificationAdapter();
        var repository = new RecordingNotificationEventRepository();
        var viewModel = new RulesViewModel(database, adapter, repository);

        await viewModel.LoadRulesAsync();
        var item = Assert.Single(viewModel.Rules);

        await viewModel.TestTimeLimitRuleAsync(item);

        var expectedMessage = "Du hast dein Zeitlimit fuer Social Media erreicht (30 Minuten). Was brauchst du gerade, bevor du weitermachst?";
        Assert.True(item.IsSoftInterventionVisible);
        Assert.Equal("Zeitlimit-Ueberschreitung gespeichert.", item.StatusMessage);
        Assert.Equal(expectedMessage, item.InterventionMessage);
        Assert.Equal(RulesViewModel.TimeLimitInterventionTitle, adapter.Title);
        Assert.Equal(expectedMessage, adapter.Message);

        var savedEvent = Assert.Single(repository.SavedEvents);
        Assert.Equal(RulesViewModel.TimeLimitInterventionTitle, savedEvent.Title);
        Assert.Equal(expectedMessage, savedEvent.Message);
        Assert.Equal($"app://rules/time-limit/{rule.Id}/exceeded", savedEvent.AppReference);
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
