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
        var rule = CreateValidRule();

        Assert.True(rule.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTitleIsEmpty()
    {
        var rule = CreateValidRule();
        rule.Title = string.Empty;

        Assert.False(rule.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTitleIsWhitespace()
    {
        var rule = CreateValidRule();
        rule.Title = "   ";

        Assert.False(rule.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTimeLimitMinutesIsZero()
    {
        var rule = CreateValidRule();
        rule.TimeLimitMinutes = 0;

        Assert.False(rule.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTimeLimitMinutesIsNegative()
    {
        var rule = CreateValidRule();
        rule.TimeLimitMinutes = -5;

        Assert.False(rule.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTargetApplicationIsEmpty()
    {
        var rule = CreateValidRule();
        rule.TargetApplication = string.Empty;

        Assert.False(rule.IsValid());
    }
}
