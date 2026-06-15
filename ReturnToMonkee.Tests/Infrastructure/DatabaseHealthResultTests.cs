using ReturnToMonkee.Infrastructure.Persistence;

namespace ReturnToMonkee.Tests.Infrastructure;

public sealed class DatabaseHealthResultTests
{
    [Fact]
    public void Ready_ReturnsIsReadyTrueWithExpectedMessage()
    {
        var result = DatabaseHealthResult.Ready();

        Assert.True(result.IsReady);
        Assert.Equal("DB bereit", result.Message);
        Assert.Null(result.Details);
    }

    [Fact]
    public void Unavailable_ReturnsIsReadyFalseWithDetailsAndMessage()
    {
        const string errorDetails = "connection refused";

        var result = DatabaseHealthResult.Unavailable(errorDetails);

        Assert.False(result.IsReady);
        Assert.Equal("DB nicht verfuegbar", result.Message);
        Assert.Equal(errorDetails, result.Details);
    }

    // TODO #12: Rule-Validierungstests hierhin, sobald UsageRule-Domain-Modell existiert.
}
