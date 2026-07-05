using ReturnToMonkee.Infrastructure.Persistence.Entities;
using ReturnToMonkee.Infrastructure.Persistence.Repositories;
using ReturnToMonkee.Tests.Helpers;

namespace ReturnToMonkee.Tests.Infrastructure.Persistence.Repositories;

public sealed class UserSettingsRepositoryTests
{
    // Jeder Test: eigene In-memory-DB → vollständige Isolation
    private static UserSettingsRepository CreateRepository()
        => new(new InMemoryLocalDatabase());

    [Fact]
    public async Task GetAsync_ReturnsSleepTimeOf22h_WhenNothingPersisted()
    {
        // Arrange
        var repo = CreateRepository();

        // Act
        var settings = await repo.GetAsync();

        // Assert
        Assert.Equal(TimeSpan.FromHours(22), settings.SleepTime);
    }

    [Fact]
    public async Task GetAsync_ReturnsSleepReminderEnabled_WhenNothingPersisted()
    {
        var repo = CreateRepository();

        var settings = await repo.GetAsync();

        Assert.True(settings.SleepReminderEnabled);
    }

    [Fact]
    public async Task SaveAsync_PersistsSleepTime_AndGetAsyncReturnsIt()
    {
        // Arrange
        var repo = CreateRepository();
        var expected = new TimeSpan(23, 30, 0);
        var settings = await repo.GetAsync();
        settings.SleepTime = expected;

        // Act
        await repo.SaveAsync(settings);
        var reloaded = await repo.GetAsync();

        // Assert
        Assert.Equal(expected, reloaded.SleepTime);
    }

    [Fact]
    public async Task SaveAsync_PersistsSleepReminderEnabled()
    {
        var repo = CreateRepository();
        var settings = await repo.GetAsync();
        settings.SleepReminderEnabled = false;

        await repo.SaveAsync(settings);
        var reloaded = await repo.GetAsync();

        Assert.False(reloaded.SleepReminderEnabled);
    }

    [Fact]
    public async Task GetAsync_ReturnsSleepReminderLeadMinutesOfZero_WhenNothingPersisted()
    {
        var repo = CreateRepository();

        var settings = await repo.GetAsync();

        Assert.Equal(0, settings.SleepReminderLeadMinutes);
    }

    [Fact]
    public async Task SaveAsync_PersistsSleepReminderLeadMinutes()
    {
        var repo = CreateRepository();
        var settings = await repo.GetAsync();
        settings.SleepReminderLeadMinutes = 30;

        await repo.SaveAsync(settings);
        var reloaded = await repo.GetAsync();

        Assert.Equal(30, reloaded.SleepReminderLeadMinutes);
    }

    [Fact]
    public async Task SaveAsync_OverwritesPreviousValue_WhenCalledTwice()
    {
        // Arrange
        var repo = CreateRepository();
        var s = await repo.GetAsync();
        s.SleepTime = new TimeSpan(21, 0, 0);
        await repo.SaveAsync(s);

        // Act
        s.SleepTime = new TimeSpan(23, 0, 0);
        await repo.SaveAsync(s);
        var reloaded = await repo.GetAsync();

        // Assert
        Assert.Equal(new TimeSpan(23, 0, 0), reloaded.SleepTime);
    }

    [Fact]
    public async Task GetAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        // Arrange
        var repo = CreateRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.GetAsync(cts.Token));
    }

    [Fact]
    public async Task SaveAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        // Arrange
        var repo = CreateRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.SaveAsync(new UserSettingsEntity(), cts.Token));
    }
}
