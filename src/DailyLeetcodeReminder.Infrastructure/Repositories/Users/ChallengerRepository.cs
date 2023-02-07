using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DailyLeetcodeReminder.Infrastructure.Repositories;

public class ChallengerRepository : IChallengerRepository
{
    private readonly ApplicationDbContext applicationDbContext;

    public ChallengerRepository(ApplicationDbContext applicationDbContext)
    {
        this.applicationDbContext = applicationDbContext;
    }

    public async Task<Challenger> SelectUserByTelegramIdAsync(long telegramId)
    {
        return await this.applicationDbContext
            .Set<Challenger>()
            .FirstOrDefaultAsync(x => x.TelegramId == telegramId);
    }

    public async Task<Challenger> SelectUserByLeetcodeUsernameAsync(string leetcodeUsername)
    {
        return await this.applicationDbContext
            .Set<Challenger>()
            .FirstOrDefaultAsync(x => x.LeetcodeUserName == leetcodeUsername);
    }

    public async Task<Challenger> InsertChallengerAsync(Challenger challenger)
    {
        var userEntityEntry = await this.applicationDbContext
            .Set<Challenger>()
            .AddAsync(challenger);
        
        await this.applicationDbContext
            .SaveChangesAsync();
        
        return userEntityEntry.Entity;
    }

    public async Task UpdateChallengerAsync(Challenger challenger)
    {
        this.applicationDbContext
            .Set<Challenger>()
            .Update(challenger);

        await this.applicationDbContext
            .SaveChangesAsync();
    }

    public async Task<List<ChallengerWithNoAttempt>> SelectUsersWithNoAttemptsAsync()
    {
        string sql = "select " +
            "ch.TelegramId," +
            "ch.TotalSolvedProblems," +
            "d.SolvedProblems " +
            "ch.LeetcodeUserName " +
            "from Challengers as ch " +
            "inner join DailyAttempts as d " +
            $"on ch.TelegramId = d.UserId where d.Date = {DateTime.Now.Date} and d.SolvedProblems = 0";

        return await this.applicationDbContext
            .Database
            .SqlQueryRaw<ChallengerWithNoAttempt>(sql)
            .ToListAsync();
    }
}