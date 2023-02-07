using DailyLeetcodeReminder.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DailyLeetcodeReminder.Infrastructure.Repositories;

public class AttemptRepository : IAttemptRepository
{
    private readonly ApplicationDbContext applicationDbContext;

    public AttemptRepository(ApplicationDbContext applicationDbContext)
    {
        this.applicationDbContext = applicationDbContext;
    }

    public async Task MarkDailyAttemptsAsync(List<long> challengerIds)
    {
        string sql = $"update DailyAttempts " +
            $"set SolvedProblems = SolvedProblems + 1 " +
            $"where UserId in ({string.Join(',', challengerIds)})";

        await this.applicationDbContext.Database.ExecuteSqlRawAsync(sql);
    }
}