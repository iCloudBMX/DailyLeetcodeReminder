using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Infrastructure.BackgroundServices;

public class DailyReportBackgroundService : BackgroundService
{
    private Timer timer = null;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public DailyReportBackgroundService(
        IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;        
    }

    private async void GenerateReportAsync(object? state)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var challengerRepository = scope.ServiceProvider
            .GetRequiredService<IChallengerRepository>();

        var leetcodeBroker = scope.ServiceProvider
            .GetRequiredService<ILeetCodeBroker>();

        var telegramBotClient = scope.ServiceProvider
            .GetRequiredService<ITelegramBotClient>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        long groupId = long.Parse(configuration
            .GetSection("TelegramBot:GroupId").Value);

        // select list of active challengers
        List<Challenger> activeChallengers = await challengerRepository
            .SelectActiveChallengersAsync();

        // get total solved problems from leetcode
        foreach (var activeChallenger in activeChallengers)
        {
            var totalSolvedProblemsCount = await leetcodeBroker
                .GetTotalSolvedProblemsCountAsync(activeChallenger.LeetcodeUserName);

            // update solved problems count
            int difference = totalSolvedProblemsCount - activeChallenger.TotalSolvedProblems;

            // if user hasn't solved any problem, decrease attempts count
            if (difference == 0)
            {
                activeChallenger.Attempts--;
            }

            // if attempts count <= 0 block the challenger
            if (activeChallenger.Attempts <= 0)
            {
                activeChallenger.Status = UserStatus.Inactive;
            }
            else
            {
                // update total solved problems
                activeChallenger.TotalSolvedProblems = totalSolvedProblemsCount;
            }

            if (activeChallenger.DailyAttempts.Count() > 0)
            {
                activeChallenger.DailyAttempts.First().SolvedProblems = difference;
            }

            // initialize the next day attempts
            activeChallenger.DailyAttempts.Add(new DailyAttempt
            {
                Date = DateTime.Now.Date,
                SolvedProblems = 0
            });
        }

        await challengerRepository.SaveChangesAsync();

        // send report to the group
        await SendDailyReportAsync(
            telegramBotClient,
            activeChallengers,
            groupId);
    }

    private static async Task SendDailyReportAsync(
        ITelegramBotClient telegramBotClient,
        List<Challenger> activeChallengers,
        long groupId)
    {
        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("Report of today");
        messageBuilder.AppendLine("Username\t|Heart\t|Today\t|Total\n");

        foreach (var challenger in activeChallengers)
        {
            messageBuilder.Append($"{challenger.LeetcodeUserName}\t|");
            messageBuilder.Append($"{challenger.Attempts}\t|");
            messageBuilder.Append($"{challenger.DailyAttempts.First().SolvedProblems}\t|");
            messageBuilder.Append($"{challenger.TotalSolvedProblems}\n");
        }

        await telegramBotClient.SendTextMessageAsync(groupId, messageBuilder.ToString());
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timeDiff = TimeOnly.Parse("00:00:00") - TimeOnly.FromDateTime(DateTime.Now);
        this.timer = new Timer(GenerateReportAsync, null, timeDiff, TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        this.timer.Dispose();
        base.Dispose();
    }
}