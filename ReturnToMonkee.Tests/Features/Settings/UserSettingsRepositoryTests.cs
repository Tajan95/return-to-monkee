using ReturnToMonkee.Features.Settings;
using ReturnToMonkee.Tests.Helpers;

namespace ReturnToMonkee.Tests.Features.Settings;

public sealed class UserSettingsRepositoryTests
{
    // Jeder Test: eigene In-memory-DB → vollständige Isolation
    private static UserSettingsRepository CreateRepository()
        => new(new InMemoryLocalDatabase());

    [Fact]
    public async Task GetAsync_ReturnsSleepTimeOf22h_WhenNothingPersisted()
    {
        var repo = CreateRepository();

        var settings = await repo.GetAsync();

        Assert.Equal(TimeSpan.FromHours(22), settings.SleepTime);
    }

    [Fact]
    public async Task SaveAsync_PersistsSleepTime_AndGetAsyncReturnsIt()
    {
        var repo = CreateRepository();
        var expected = new TimeSpan(23, 30, 0);

        var settings = await repo.GetAsync();
        settings.SleepTime = expected;
        await repo.SaveAsync(settings);

        var reloaded = await repo.GetAsync();
        Assert.Equal(expected, reloaded.SleepTime);
    }

    [Fact]
    public async Task SaveAsync_OverwritesPreviousValue_WhenCalledTwice()
    {
        var repo = CreateRepository();

        var s = await repo.GetAsync();
        s.SleepTime = new TimeSpan(21, 0, 0);
        await repo.SaveAsync(s);

        s.SleepTime = new TimeSpan(23, 0, 0);
        await repo.SaveAsync(s);

        var reloaded = await repo.GetAsync();
        Assert.Equal(new TimeSpan(23, 0, 0), reloaded.SleepTime);
    }

    [Fact]
    public async Task GetAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        var repo = CreateRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.GetAsync(cts.Token));
    }

    [Fact]
    public async Task SaveAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        var repo = CreateRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repo.SaveAsync(new UserSettings(), cts.Token));
    }
}
