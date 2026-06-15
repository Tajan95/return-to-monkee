namespace ReturnToMonkee.Infrastructure.Persistence.Repositories;

public interface ITimeLimitRuleRepository
{
    Task SaveInitialTimeLimitRuleAsync(
        string category,
        int timeLimitMinutes,
        CancellationToken cancellationToken = default);

    Task<List<global::TimeLimitRule>> GetAllAsync(
        CancellationToken cancellationToken = default);
}