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

    public async Task InitializeDailyAttemptsAsync()
    {
        string sql = "INSERT INTO DailyAttempts " +
            "SELECT CAST(GETDATE() AS Date), TelegramId, 0 " +
            "FROM Challengers WHERE Status = 0";

        await this.applicationDbContext.Database.ExecuteSqlRawAsync(sql);
    }

    public async Task MarkDailyAttemptsAsync(List<long> challengerIds)
    {
        string sql = @$"update ""DailyAttempts"" " +
            @$"set ""SolvedProblems"" = ""SolvedProblems"" + 1 " +
            @$"where ""UserId"" in ({string.Join(',', challengerIds)}) " +
            @$"and ""Date"" = '{DateOnly.FromDateTime(DateTime.Now)}'";

        await this.applicationDbContext.Database.ExecuteSqlRawAsync(sql);
    }
}