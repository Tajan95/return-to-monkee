using ReturnToMonkee.Infrastructure.Notifications;

namespace ReturnToMonkee.Tests.Infrastructure;

public sealed class NotificationAdapterTests
{
    [Fact]
    public async Task SendAsync_InvokesDisplayFunction_WithCorrectTitleAndMessage()
    {
        string? capturedTitle = null;
        string? capturedMessage = null;

        var adapter = new MockNotificationAdapter(
            display: (t, m) => { capturedTitle = t; capturedMessage = m; return Task.CompletedTask; },
            prompt: (_, _, _, _) => Task.FromResult(false));

        await adapter.SendAsync("Titel", "Nachricht");

        Assert.Equal("Titel", capturedTitle);
        Assert.Equal("Nachricht", capturedMessage);
    }

    [Fact]
    public async Task PromptAsync_ReturnsTrue_WhenConfirmCallbackReturnsTrue()
    {
        var adapter = new MockNotificationAdapter(
            display: (_, _) => Task.CompletedTask,
            prompt: (_, _, _, _) => Task.FromResult(true));

        var result = await adapter.PromptAsync("Titel", "Nachricht", "Ja", "Nein");

        Assert.True(result);
    }

    [Fact]
    public async Task PromptAsync_ReturnsFalse_WhenConfirmCallbackReturnsFalse()
    {
        var adapter = new MockNotificationAdapter(
            display: (_, _) => Task.CompletedTask,
            prompt: (_, _, _, _) => Task.FromResult(false));

        var result = await adapter.PromptAsync("Titel", "Nachricht", "Ja", "Nein");

        Assert.False(result);
    }

    [Fact]
    public async Task SendAsync_ThrowsOperationCanceled_WhenCancelled()
    {
        var adapter = new MockNotificationAdapter(
            display: (_, _) => Task.CompletedTask,
            prompt: (_, _, _, _) => Task.FromResult(false));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => adapter.SendAsync("x", "y", cts.Token));
    }
}
