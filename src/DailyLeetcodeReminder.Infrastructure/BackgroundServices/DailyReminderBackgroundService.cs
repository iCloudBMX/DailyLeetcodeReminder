using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Infrastructure.BackgroundServices;

public class DailyReminderBackgroundService : BackgroundService
{
    private readonly PeriodicTimer periodicTimer;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public DailyReminderBackgroundService(
        IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        short reminderInHours = short.Parse(configuration.GetSection("Timer:Reminder").Value);
        this.periodicTimer = new PeriodicTimer(TimeSpan.FromHours(reminderInHours));
        this.serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = this.serviceScopeFactory.CreateScope();

            var challengerRepository = scope.ServiceProvider
                .GetRequiredService<IChallengerRepository>();

            var leetcodeBroker = scope.ServiceProvider
                .GetRequiredService<ILeetCodeBroker>();

            var telegramBotClient = scope.ServiceProvider
                .GetRequiredService<ITelegramBotClient>();

            var attemptRepository = scope.ServiceProvider
                .GetRequiredService<IAttemptRepository>();

            while (await this.periodicTimer.WaitForNextTickAsync())
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
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}