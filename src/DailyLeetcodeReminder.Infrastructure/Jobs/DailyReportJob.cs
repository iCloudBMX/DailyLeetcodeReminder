using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Quartz;
using System.Text;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Infrastructure.Jobs;

public class DailyReportJob : IJob
{
    private readonly IChallengerRepository challengerRepository;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ILeetCodeBroker leetcodeBroker;
    private readonly IConfiguration configuration;

    public DailyReportJob(
        IChallengerRepository challengerRepository,
        ITelegramBotClient telegramBotClient,
        ILeetCodeBroker leetcodeBroker,
        IConfiguration configuration)
    {
        this.challengerRepository = challengerRepository;
        this.telegramBotClient = telegramBotClient;
        this.leetcodeBroker = leetcodeBroker;
        this.configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
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
            if (difference == 0 && 
                activeChallenger.CreatedAt.Date < DateTime.Now.Date.AddDays(-1))
            {
                activeChallenger.Heart--;
            }

            // if attempts count <= 0 block the challenger
            if(activeChallenger.Heart <= 0)
            {
                activeChallenger.Status = UserStatus.Inactive;

                await telegramBotClient.BanChatMemberAsync(
                    chatId: groupId,
                    userId: activeChallenger.TelegramId);
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
            messageBuilder.Append($"{challenger.Heart}\t|");
            messageBuilder.Append($"{challenger.DailyAttempts.First().SolvedProblems}\t|");
            messageBuilder.Append($"{challenger.TotalSolvedProblems}\n");
        }

        await telegramBotClient.SendTextMessageAsync(groupId, messageBuilder.ToString());
    }
}