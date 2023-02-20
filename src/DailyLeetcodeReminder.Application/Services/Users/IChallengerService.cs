using DailyLeetcodeReminder.Domain.Entities;

namespace DailyLeetcodeReminder.Application.Services;

public interface IChallengerService
{
    Task<Challenger> AddUserAsync(Challenger challenger);
    Task<Challenger> RetrieveChallengerByTelegramIdAsync(long telegramId);
    Task<Challenger> RetrieveChallengerByLeetcodeUsernameAsync(string leetcodeUsername);
    Task<Challenger> ModifyChallengerAsync(Challenger challenger);
    Task<List<Challenger>> RetrieveChallengers();
    Task<int> CurrentSolvedProblemsAsync(string leetcodeUsername);
    Task<Challenger> UserWithAttemptsWeekyAsync(long userId);
}