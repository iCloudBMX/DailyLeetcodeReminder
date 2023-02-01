namespace DailyLeetcodeReminder.Infrastructure.Services;

public interface ILeetCodeDataExtractorService
{
    Task<int> ExtractSolvedProblemsCountAsync(string leetcodeUsername);
}