using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee.Tests.Infrastructure;

public sealed class DatabaseHealthResultTests
{
    [Fact]
    public void Ready_ReturnsIsReadyTrueWithExpectedMessage()
    {
        // Act
        var result = DatabaseHealthResult.Ready();

        // Assert
        Assert.True(result.IsReady);
        Assert.Equal("DB bereit", result.Message);
        Assert.Null(result.Details);
    }

    [Fact]
    public void Unavailable_ReturnsIsReadyFalseWithDetailsAndMessage()
    {
        // Arrange
        const string errorDetails = "connection refused";

        // Act
        var result = DatabaseHealthResult.Unavailable(errorDetails);

        // Assert
        Assert.False(result.IsReady);
        Assert.Equal("DB nicht verfügbar", result.Message);
        Assert.Equal(errorDetails, result.Details);
    }
}
