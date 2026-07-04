using Microsoft.Extensions.Logging;

namespace ReturnToMonkee.Infrastructure.Persistence;

/// <summary>
/// Legt auf ausdrueckliche Anforderung (Button in den Einstellungen) einige Demo-Zeitlimit-Regeln
/// an, damit die App fuer Praesentation und Tests schnell mit Beispieldaten befuellt werden kann.
/// Wird bewusst NICHT automatisch beim App-Start ausgefuehrt.
/// </summary>
public sealed class DemoDataSeeder
{
    // Feste IDs -> Seeden ist idempotent (mehrfaches Druecken legt keine Duplikate an).
    private static readonly (Guid Id, string Category, int Minutes)[] DemoRules =
    {
        (Guid.Parse("f4e8d4f7-7f6f-47d3-bdb1-4f2b60d5f01b"), "Social Media", 30),
        (Guid.Parse("b1c2d3e4-5f60-4718-9a2b-3c4d5e6f7a8b"), "Video/Streaming", 45),
        (Guid.Parse("0a6a91b9-1d9b-4c21-9e36-2b4cb5f8d8d3"), "Gaming", 60),
    };

    private readonly ILocalDatabase localDatabase;
    private readonly ILogger<DemoDataSeeder> logger;

    public DemoDataSeeder(ILocalDatabase localDatabase, ILogger<DemoDataSeeder> logger)
    {
        this.localDatabase = localDatabase;
        this.logger = logger;
    }

    /// <summary>
    /// Prueft, ob bereits alle Demo-Regeln vorhanden sind (dann waere ein erneutes Seeden ein No-Op).
    /// </summary>
    public async Task<bool> AreDemoRulesPresentAsync(CancellationToken cancellationToken = default)
    {
        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<TimeLimitRule>();

        foreach (var (id, _, _) in DemoRules)
        {
            var exists = await connection.Table<TimeLimitRule>()
                .Where(rule => rule.Id == id)
                .CountAsync();

            if (exists == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Legt die Demo-Zeitlimit-Regeln an (idempotent ueber feste IDs) und gibt die Anzahl der
    /// tatsaechlich neu erstellten Regeln zurueck.
    /// </summary>
    public async Task<int> SeedRulesAsync(CancellationToken cancellationToken = default)
    {
        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<TimeLimitRule>();

        var created = 0;

        foreach (var (id, category, minutes) in DemoRules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exists = await connection.Table<TimeLimitRule>()
                .Where(rule => rule.Id == id)
                .CountAsync();

            if (exists > 0)
            {
                continue;
            }

            await connection.InsertAsync(new TimeLimitRule
            {
                Id = id,
                Title = $"{category} begrenzen",
                Description = $"Demo-Regel: tägliches Zeitlimit für {category} ({minutes} Minuten)",
                IsEnabled = true,
                TargetApplication = category,
                TimeLimitMinutes = minutes,
            });

            created++;
        }

        logger.LogInformation("Demo rules seeded: {Created} new of {Total}", created, DemoRules.Length);
        return created;
    }
}
