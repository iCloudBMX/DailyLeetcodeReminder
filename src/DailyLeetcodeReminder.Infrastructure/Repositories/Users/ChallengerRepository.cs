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
}