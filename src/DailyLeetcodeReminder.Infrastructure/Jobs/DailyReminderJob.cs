using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Quartz;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Infrastructure.Jobs;

public class DailyReminderJob : IJob
{
    private readonly IChallengerRepository challengerRepository;
    private readonly ILeetCodeBroker leetcodeBroker;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly IAttemptRepository attemptRepository;

    public DailyReminderJob(
        IChallengerRepository challengerRepository,
        ILeetCodeBroker leetcodeBroker,
        ITelegramBotClient telegramBotClient,
        IAttemptRepository attemptRepository)
    {
        this.challengerRepository = challengerRepository;
        this.leetcodeBroker = leetcodeBroker;
        this.telegramBotClient = telegramBotClient;
        this.attemptRepository = attemptRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
           List<ChallengerWithNoAttempt> challengers =
                await challengerRepository.SelectUsersWithNoAttemptsAsync();

            List<long> challengersWithNoAttempts = new();
            List<long> challengersHasAttempts = new();

            foreach (var challenger in challengers)
            {
                int totalSolvedProblemsCount = await leetcodeBroker
                    .GetTotalSolvedProblemsCountAsync(challenger.LeetcodeUserName);

                int difference = totalSolvedProblemsCount - challenger.TotalSolvedProblems;

                if (difference == 0)
                {
                    challengersWithNoAttempts.Add(challenger.TelegramId);
                    continue;
                }

                challengersHasAttempts.Add(challenger.TelegramId);
            }

            if (challengersHasAttempts.Count > 0)
            {
                await attemptRepository
                    .MarkDailyAttemptsAsync(challengersHasAttempts);
            }

            foreach (long telegramId in challengersWithNoAttempts)
            {
                await telegramBotClient.SendTextMessageAsync(
                    chatId: telegramId,
                    text: "You haven't solved any problem yet. If you want to stay, try to solve any problem");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}