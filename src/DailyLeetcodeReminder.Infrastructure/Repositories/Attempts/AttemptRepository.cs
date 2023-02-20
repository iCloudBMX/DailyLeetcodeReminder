using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

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
        await this.applicationDbContext.Set<DailyAttempt>()
            .Where(da => challengerIds.Any(id => id == da.UserId))
            .Where(da => da.Date.Date == DateTime.Now.Date)
            .ExecuteUpdateAsync(o => o.SetProperty(
                da => da.SolvedProblems,
                da => da.SolvedProblems + 1));
    }
}