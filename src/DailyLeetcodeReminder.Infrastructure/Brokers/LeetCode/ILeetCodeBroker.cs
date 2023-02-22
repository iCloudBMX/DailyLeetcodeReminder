using DailyLeetcodeReminder.Infrastructure.Models;

namespace DailyLeetcodeReminder.Infrastructure.Brokers.LeetCode;

public interface ILeetCodeBroker
{
    Task<int> GetTotalSolvedProblemsCountAsync(string leetcodeUsername);
    Task<DailyProblem> GetDailyProblemAsync();
}