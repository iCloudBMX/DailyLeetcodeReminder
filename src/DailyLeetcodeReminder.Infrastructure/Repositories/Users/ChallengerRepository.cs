using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Domain.Exceptions;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using DailyLeetcodeReminder.Infrastructure.Jobs;
using Microsoft.Data.SqlClient;
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

        await this.SaveChangesAsync();
    }

    public async Task<List<ChallengerWithNoAttempt>> SelectUsersWithNoAttemptsAsync()
    {
            return await this.applicationDbContext
            .Set<Challenger>()
            .Include(ch => ch.DailyAttempts
                .Where(da => da.Date == DateTime.Now.Date.AddDays(-1))
                .Where(da => da.SolvedProblems == 0))
            .Where(ch => ch.Status == UserStatus.Active)
            .Select(ch => new ChallengerWithNoAttempt
            {
                LeetcodeUserName = ch.LeetcodeUserName,
                TelegramId = ch.TelegramId,
                TotalSolvedProblems = ch.TotalSolvedProblems
            })
            .ToListAsync();
    }

    public async Task<List<Challenger>> SelectActiveChallengersAsync()
    {
        return await this.applicationDbContext
            .Set<Challenger>()
            .Include(ch => ch.DailyAttempts
                .Where(da => da.Date == DateTime.Now.Date.AddDays(-1)))
            .Where(ch => ch.Status == UserStatus.Active)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.applicationDbContext
                    .SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlException)
            {
                if (sqlException.Number == 2601 && 
                    sqlException.Message.Contains("LeetcodeUserName") &&
                    sqlException.Message.Contains("TelegramId"))
                {
                    throw new AlreadyExictException();
                }
                else if (sqlException.Number == 2601 && 
                    sqlException.Message.Contains("LeetcodeUserName"))
                {
                    throw new DuplicateLeetCodeUserNameException();
                }
                else if (sqlException.Number == 2601 && 
                        sqlException.Message.Contains("TelegramId"))
                {
                    throw new DuplicateTelegramIdException();
                }
                else
                {
                    throw;
                }
            }
            else
            {
                throw;
            }
        }
    }
}