using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public class GoalsRepository : IGoalsRepository
{
    private readonly ILocalDatabase dbProvider;

    public GoalsRepository(ILocalDatabase dbProvider)
    {
        this.dbProvider = dbProvider;
    }

    public async Task<List<GoalEntity>> GetAllGoalsAsync()
    {
        var db = await dbProvider.GetConnectionAsync();

        await db.CreateTableAsync<GoalEntity>();
        await db.CreateTableAsync<UserGoalEntity>();

        return await db.Table<GoalEntity>().ToListAsync();
    }

    public async Task<List<int>> GetSelectedGoalIdsAsync()
    {
        var db = await dbProvider.GetConnectionAsync();

        await db.CreateTableAsync<UserGoalEntity>();

        var selected = await db.Table<UserGoalEntity>().ToListAsync();

        return selected.Select(x => x.GoalId).ToList();
    }

    public async Task SaveSelectedGoalsAsync(List<int> goalIds)
    {
        var db = await dbProvider.GetConnectionAsync();

        await db.CreateTableAsync<UserGoalEntity>();

        await db.DeleteAllAsync<UserGoalEntity>();

        var entities = goalIds.Select(id => new UserGoalEntity
        {
            GoalId = id
        });

        await db.InsertAllAsync(entities);
    }

    public async Task SeedAsync()
    {
        var db = await dbProvider.GetConnectionAsync();
        await db.CreateTableAsync<GoalEntity>();

        var count = await db.Table<GoalEntity>().CountAsync();
        if (count == 0)
        {
            await db.InsertAllAsync(new[]
            {
                new GoalEntity { Title = "Weniger Social Media" },
                new GoalEntity { Title = "Mehr Fokus" },
                new GoalEntity { Title = "Besser schlafen" },
                new GoalEntity { Title = "Mehr Energie" }
            });
        }
    }
}