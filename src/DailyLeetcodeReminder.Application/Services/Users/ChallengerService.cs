using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;

namespace DailyLeetcodeReminder.Application.Services;

public class ChallengerService : IChallengerService
{
    private readonly IChallengerRepository challengerRepository;
    private readonly ILeetCodeBroker leetcodeBroker;
    private const short maxAttempts = 3;

    public ChallengerService(
        IChallengerRepository userRepository,
        ILeetCodeBroker leetcodeBroker)
    {
        this.challengerRepository = userRepository;
        this.leetcodeBroker = leetcodeBroker;
    }

    public async Task<Challenger> AddUserAsync(Challenger challenger)
    {
        challenger.Heart = maxAttempts;

        challenger.TotalSolvedProblems = await leetcodeBroker
            .GetTotalSolvedProblemsCountAsync(challenger.LeetcodeUserName);

        Challenger insertedChallenger = await this.challengerRepository
            .InsertChallengerAsync(challenger);

        return insertedChallenger;
    }
    public async Task<Challenger> RetrieveChallengerByTelegramIdAsync(long telegramId)
    {
        Challenger storageChallenger = await this.challengerRepository
            .SelectUserByTelegramIdAsync(telegramId);

        if(storageChallenger is null)
        {
            throw new Exception("Challenger is not found");
        }

        return storageChallenger;
    }
    public async Task<Challenger> RetrieveChallengerByLeetcodeUsernameAsync(string leetcodeUsername)
    {
        Challenger storageChallenger = await this.challengerRepository
            .SelectUserByLeetcodeUsernameAsync(leetcodeUsername);

        if (storageChallenger is null)
        {
            throw new Exception("Challenger is not found");
        }

        return storageChallenger;
    }
    public async Task<Challenger> ModifyChallengerAsync(Challenger challenger)
    {
        Challenger storageChallenger = await this.challengerRepository
            .SelectUserByTelegramIdAsync(challenger.TelegramId);

        if (storageChallenger is null)
        {
            throw new Exception("Challenger not found");
        }

        storageChallenger.FirstName = challenger.FirstName;
        storageChallenger.LastName = challenger.LastName;
        storageChallenger.TotalSolvedProblems = challenger.TotalSolvedProblems;
        storageChallenger.Heart = challenger.Heart;
        storageChallenger.Status = challenger.Status;

        await this.challengerRepository.UpdateChallengerAsync(storageChallenger);

        return storageChallenger;
    }
    public async Task<List<Challenger>> RetrieveChallengers()
    {
        return await challengerRepository.SelectActiveChallengersAsync();
    }
    public async Task<int> CurrentSolvedProblemsAsync(string leetcodeUsername)
    {
        return await leetcodeBroker.GetTotalSolvedProblemsCountAsync(leetcodeUsername);
    }
    public async Task<Challenger> WeeklyUserAttempts(long userId)
    {
        return await this.challengerRepository
                .SelectUserWithWeeklyAttempts(userId);
    }
}