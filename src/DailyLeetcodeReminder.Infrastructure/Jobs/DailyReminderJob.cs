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

            var timeSpan= DateTime.Now.Date.AddDays(1) - DateTime.Now;


            foreach (long telegramId in challengersWithNoAttempts)
            {
                timeSpan = DateTime.Now.Date.AddDays(1) - DateTime.Now;
                var timeSpanStr = $"{timeSpan.ToString("hh")}:{timeSpan.ToString("mm")}:{timeSpan.ToString("ss")}";

                await telegramBotClient.SendTextMessageAsync(
                    chatId: telegramId,
                    text: "Siz hali hech qanday masala ishlaganiz yo'q. Agar guruhda qolishni istasangiz, har qanday masalani ishlashga harakat qiling."
                    + $"\nSizda {timeSpanStr} vaqt qoldi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}