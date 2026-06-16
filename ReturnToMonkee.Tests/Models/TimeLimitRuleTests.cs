namespace ReturnToMonkee.Tests.Models;

/// <summary>
/// Rule-Validierungstests, nachgezogen aus #14 (AC: "Mindestens 2 Rule-Validierungstests sind grün"),
/// blockiert bis #12 das Domain-Modell lieferte. <see cref="TimeLimitRule"/> ist die einzige
/// konkrete Regel im aktuellen Domain-Modell.
/// </summary>
public sealed class TimeLimitRuleTests
{
    private static TimeLimitRule CreateValidRule() => new()
    {
        Title = "Social Media Limit",
        TimeLimitMinutes = 30,
        TargetApplication = "Social Media"
    };

    [Fact]
    public void IsValid_ReturnsTrue_WhenTitleTimeLimitAndTargetApplicationAreSet()
    {
        // Arrange
        var rule = CreateValidRule();

        // Act
        var isValid = rule.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTitleIsEmpty()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Title = string.Empty;

        // Act
        var isValid = rule.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTitleIsWhitespace()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.Title = "   ";

        // Act
        var isValid = rule.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTimeLimitMinutesIsZero()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.TimeLimitMinutes = 0;

        // Act
        var isValid = rule.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTimeLimitMinutesIsNegative()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.TimeLimitMinutes = -5;

        // Act
        var isValid = rule.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTargetApplicationIsEmpty()
    {
        // Arrange
        var rule = CreateValidRule();
        rule.TargetApplication = string.Empty;

        // Act
        var isValid = rule.IsValid();

        // Assert
        Assert.False(isValid);
    }
}
