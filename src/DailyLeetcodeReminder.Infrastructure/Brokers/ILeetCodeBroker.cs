using DailyLeetcodeReminder.Infrastructure.Models;

namespace DailyLeetcodeReminder.Infrastructure.Services;

public interface ILeetCodeBroker
{
    Task<int> GetTotalSolvedProblemsCountAsync(string leetcodeUsername);
    Task<string> GetDailyChallengeUrlAsync();
}