using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Brokers.LeetCode;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

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
        List<ChallengerWithNoAttempt> challengers =
                await challengerRepository.SelectUsersHasNoAttemptsAsync();

        List<long> challengersHasNoAttempts = new();
        List<long> challengersHasAttempts = new();

        foreach (var challenger in challengers)
        {
            int totalSolvedProblemsCount = await leetcodeBroker
                .GetTotalSolvedProblemsCountAsync(challenger.LeetcodeUserName);

            int todaySolvedProblems = totalSolvedProblemsCount - challenger.TotalSolvedProblems;

            if (todaySolvedProblems == 0)
            {
                challengersHasNoAttempts.Add(challenger.TelegramId);
                continue;
            }

            challengersHasAttempts.Add(challenger.TelegramId);
        }

        if (challengersHasAttempts.Count > 0)
        {
            await attemptRepository
                .MarkDailyAttemptsAsync(challengersHasAttempts);
        }

        var endTime = DateTime.Today.AddDays(1);
        var timeRemaining = endTime - DateTime.Now;

        foreach (long telegramId in challengersHasNoAttempts)
        {
            var timeRemainingStr = timeRemaining.ToString(@"hh\:mm\:ss");

            string message = "Siz hali hech qanday masala ishlaganiz yo'q. " +
                "Agar guruhda qolishni istasangiz, har qanday masalani ishlashga harakat qiling."
                + $"\nSizda <b>{timeRemainingStr}</b> vaqt qoldi";

            await telegramBotClient.SendTextMessageAsync(
                chatId: telegramId,
                text: message,
                parseMode: ParseMode.Html);
        }
    }
}