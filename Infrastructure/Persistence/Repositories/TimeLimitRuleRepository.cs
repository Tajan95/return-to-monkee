namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public sealed class TimeLimitRuleRepository : ITimeLimitRuleRepository
{
    private readonly ILocalDatabase localDatabase;

    public TimeLimitRuleRepository(ILocalDatabase localDatabase)
    {
        this.localDatabase = localDatabase;
    }

    public async Task SaveInitialTimeLimitRuleAsync(
        string category,
        int timeLimitMinutes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException(
                "Eine Kategorie muss angegeben werden.",
                nameof(category));
        }

        if (timeLimitMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeLimitMinutes),
                "Das Zeitlimit muss größer als 0 Minuten sein.");
        }

        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await connection.CreateTableAsync<global::TimeLimitRule>();

        // MVP-Vereinfachung:
        // Für das Onboarding wird genau eine initiale Zeitlimit-Regel gespeichert.
        // Spätere Regelverwaltung kann mehrere Regeln verwalten und bearbeiten.
        await connection.DeleteAllAsync<global::TimeLimitRule>();

        var rule = new global::TimeLimitRule
        {
            Id = Guid.NewGuid(),
            Title = $"{category} begrenzen",
            Description = $"Tägliches Zeitlimit für {category}: {timeLimitMinutes} Minuten",
            IsEnabled = true,
            TargetApplication = category,
            TimeLimitMinutes = timeLimitMinutes
        };

        await connection.InsertAsync(rule);
    }

    public async Task<List<global::TimeLimitRule>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await localDatabase.GetConnectionAsync(cancellationToken);

        await connection.CreateTableAsync<global::TimeLimitRule>();

        return await connection.Table<global::TimeLimitRule>()
            .ToListAsync();
    }

    public async Task AddAsync(global::TimeLimitRule rule, CancellationToken cancellationToken = default)
    {
        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<global::TimeLimitRule>();
        await connection.InsertAsync(rule);
    }

    public async Task UpdateAsync(global::TimeLimitRule rule, CancellationToken cancellationToken = default)
    {
        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<global::TimeLimitRule>();
        await connection.UpdateAsync(rule);
    }

    public async Task DeleteAsync(global::TimeLimitRule rule, CancellationToken cancellationToken = default)
    {
        var connection = await localDatabase.GetConnectionAsync(cancellationToken);
        await connection.CreateTableAsync<global::TimeLimitRule>();
        await connection.DeleteAsync(rule);
    }
}