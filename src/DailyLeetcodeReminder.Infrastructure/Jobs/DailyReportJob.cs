using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DailyLeetcodeReminder.Infrastructure.Jobs;

public class DailyReportJob : IJob
{
    private readonly IChallengerRepository challengerRepository;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ILeetCodeBroker leetcodeBroker;
    private readonly ILogger<DailyReportJob> logger;

    public DailyReportJob(
        IChallengerRepository challengerRepository,
        ITelegramBotClient telegramBotClient,
        ILeetCodeBroker leetcodeBroker,
        ILogger<DailyReportJob> logger)
    {
        this.challengerRepository = challengerRepository;
        this.telegramBotClient = telegramBotClient;
        this.leetcodeBroker = leetcodeBroker;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        long groupId = long.Parse(Environment.GetEnvironmentVariable("GROUP_ID"));

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

                var chatMember = await telegramBotClient
                    .GetChatMemberAsync(groupId, activeChallenger.TelegramId);

                if(chatMember.Status is not ChatMemberStatus.Administrator and not ChatMemberStatus.Creator)
                {
                    await telegramBotClient.BanChatMemberAsync(
                        chatId: groupId,
                        userId: activeChallenger.TelegramId).ConfigureAwait(false);
                }
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
                UserId = activeChallenger.TelegramId,
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

    private async Task SendDailyReportAsync(
        ITelegramBotClient telegramBotClient,
        List<Challenger> activeChallengers,
        long groupId)
    {
        string messageBuilder = SendDailyReport(activeChallengers: activeChallengers);

        await telegramBotClient.SendTextMessageAsync(
            chatId: groupId, 
            text: messageBuilder,
            parseMode: ParseMode.Html);
    }
    private string SendDailyReport(List<Challenger> activeChallengers)
    {
        StringBuilder messageBuilder = new();

        messageBuilder.AppendLine($"Hisobot  ({DateTime.Now.ToString("dd.MMMM.yyyy")})\n");

        messageBuilder.AppendLine($"<pre>|{new string('-', 22)}" +
                                       $"|{new string('-', 7)}" +
                                       $"|{new string('-', 7)}" +
                                       $"|{new string('-', 7)}|");

        messageBuilder.AppendLine(String.Format("| {0, -20} | {1, -6}| {2, -6}| {3, -6}|",
                                                "Foydalanuvchi nomi", "Yurak", "Bugun", "Jami"));

        messageBuilder.AppendLine($"|{new string('-', 22)}" +
                                  $"|{new string('-', 7)}" +
                                  $"|{new string('-', 7)}" +
                                  $"|{new string('-', 7)}|");

        foreach (var challenger in activeChallengers)
        {
            int? SolvedProblems = challenger.DailyAttempts.Where(da => da.Date != DateTime.Now.Date)
                            .FirstOrDefault().SolvedProblems;
            if (SolvedProblems == null)
                continue;
            messageBuilder.AppendLine(String.Format("| {0, -20} | {1, -6}| {2, -6}| {3, -6}|",
                        challenger.LeetcodeUserName,
                        challenger.Heart,
                        SolvedProblems,
                        challenger.TotalSolvedProblems)); 
        }

        return messageBuilder.ToString() + "</pre>";
    }
}
