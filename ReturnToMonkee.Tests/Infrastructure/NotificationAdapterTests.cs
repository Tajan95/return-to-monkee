using ReturnToMonkee.Infrastructure.Notifications;

namespace ReturnToMonkee.Tests.Infrastructure;

public sealed class NotificationAdapterTests
{
    [Fact]
    public async Task SendAsync_InvokesDisplayFunction_WithCorrectTitleAndMessage()
    {
        // Arrange
        string? capturedTitle = null;
        string? capturedMessage = null;

        var adapter = new MockNotificationAdapter(
            display: (t, m) => { capturedTitle = t; capturedMessage = m; return Task.CompletedTask; },
            prompt: (_, _, _, _) => Task.FromResult(false));

        // Act
        await adapter.SendAsync("Titel", "Nachricht");

        // Assert
        Assert.Equal("Titel", capturedTitle);
        Assert.Equal("Nachricht", capturedMessage);
    }

    [Fact]
    public async Task PromptAsync_ReturnsTrue_WhenConfirmCallbackReturnsTrue()
    {
        // Arrange
        var adapter = new MockNotificationAdapter(
            display: (_, _) => Task.CompletedTask,
            prompt: (_, _, _, _) => Task.FromResult(true));

        // Act
        var result = await adapter.PromptAsync("Titel", "Nachricht", "Ja", "Nein");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PromptAsync_ReturnsFalse_WhenConfirmCallbackReturnsFalse()
    {
        // Arrange
        var adapter = new MockNotificationAdapter(
            display: (_, _) => Task.CompletedTask,
            prompt: (_, _, _, _) => Task.FromResult(false));

        // Act
        var result = await adapter.PromptAsync("Titel", "Nachricht", "Ja", "Nein");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendAsync_ThrowsOperationCanceled_WhenCancelled()
    {
        // Arrange
        var adapter = new MockNotificationAdapter(
            display: (_, _) => Task.CompletedTask,
            prompt: (_, _, _, _) => Task.FromResult(false));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => adapter.SendAsync("x", "y", cts.Token));
    }
}
