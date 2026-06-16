using ReturnToMonkee.Infrastructure.Persistence.Entities;
using SQLite;

namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public interface IGoalsRepository
{

    Task<List<GoalEntity>> GetAllGoalsAsync();

    Task<List<int>> GetSelectedGoalIdsAsync();

    Task SaveSelectedGoalsAsync(List<int> goalIds);

    Task SeedAsync();
}