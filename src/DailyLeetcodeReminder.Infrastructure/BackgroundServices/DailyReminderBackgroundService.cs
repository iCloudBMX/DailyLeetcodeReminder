using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Infrastructure.BackgroundServices;

public class DailyReminderBackgroundService : BackgroundService
{
    private readonly PeriodicTimer periodicTimer;
    private readonly IChallengerRepository challengerRepository;
    private readonly ILeetCodeBroker leetcodeBroker;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly IAttemptRepository attemptRepository;

    public DailyReminderBackgroundService(
        PeriodicTimer periodicTimer,
        IChallengerRepository challengerRepository,
        ILeetCodeBroker leetcodeBroker,
        ITelegramBotClient telegramBotClient,
        IAttemptRepository attemptRepository)
    {
        this.periodicTimer = periodicTimer;
        this.challengerRepository = challengerRepository;
        this.leetcodeBroker = leetcodeBroker;
        this.telegramBotClient = telegramBotClient;
        this.attemptRepository = attemptRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(await this.periodicTimer.WaitForNextTickAsync())
        {
            List<ChallengerWithNoAttempt> challengers =
                await this.challengerRepository.SelectUsersWithNoAttemptsAsync();

            List<long> challengersWithNoAttempts = new();
            List<long> challengersHasAttempts = new();

            foreach (var challenger in challengers)
            {
                int totalSolvedProblemsCount = await this.leetcodeBroker
                    .GetTotalSolvedProblemsCountAsync(challenger.LeetcodeUserName);

                int difference = totalSolvedProblemsCount - challenger.TotalSolvedProblems;

                if (difference == 0)
                {
                    challengersWithNoAttempts.Add(challenger.TelegramId);
                    continue;
                }

                challengersHasAttempts.Add(challenger.TelegramId);
            }

            if(challengersHasAttempts.Count > 0)
            {
                await this.attemptRepository
                    .MarkDailyAttemptsAsync(challengersHasAttempts);
            }

            await SendNotificationToChallengers(challengersWithNoAttempts);
        }
    }

    private async Task SendNotificationToChallengers(
        IReadOnlyList<long> challengersWithNoAttempts)
    {
        foreach (long telegramId in challengersWithNoAttempts)
        {
            await this.telegramBotClient.SendTextMessageAsync(
                chatId: telegramId,
                text: "You haven't solved any problem yet. If you want to stay, try to solve any problem");
        }
    }
}