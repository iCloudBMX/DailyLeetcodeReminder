using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text;
using Telegram.Bot;
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
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);

        List<Challenger> activeChallengers = await challengerRepository
            .SelectActiveChallengersWithAttemptsAsync();

        foreach (var activeChallenger in activeChallengers)
        {
            var totalSolvedProblemsCount = await leetcodeBroker
                .GetTotalSolvedProblemsCountAsync(activeChallenger.LeetcodeUserName);

            if(totalSolvedProblemsCount == -1)
            {
                activeChallenger.Status = UserStatus.Inactive;
                continue;
            }

            int yesterdayProblemsSolved = totalSolvedProblemsCount - activeChallenger.TotalSolvedProblems;

            // if user hasn't solved any problem, decrease attempts count
            if (yesterdayProblemsSolved == 0 && 
                activeChallenger.CreatedAt.Date < yesterday)
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
                    await RemoveMemberAsync(
                        groupId,
                        activeChallenger.TelegramId);
                }
            }
            else
            {
                // update total solved problems
                activeChallenger.TotalSolvedProblems = totalSolvedProblemsCount;
            }

            var yesterdayAttempt = activeChallenger
                .DailyAttempts
                .FirstOrDefault(da => da.Date == yesterday);

            if(yesterdayAttempt is not null)
            {
                yesterdayAttempt.SolvedProblems = yesterdayProblemsSolved;
            }

            // initialize the next day attempts
            activeChallenger.DailyAttempts.Add(new DailyAttempt
            {
                UserId = activeChallenger.TelegramId,
                Date = today,
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
        string dailyReportDetails = GetDailyReportDetails(activeChallengers: activeChallengers);

        await telegramBotClient.SendTextMessageAsync(
            chatId: groupId, 
            text: dailyReportDetails,
            parseMode: ParseMode.Html);
    }

    private string GetDailyReportDetails(List<Challenger> activeChallengers)
    {
        StringBuilder messageBuilder = new();

        messageBuilder.AppendLine($"Hisobot - <b>{DateTime.Now.ToString("dd.MM.yyyy")}</b>\n");

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

        var yesterDay = DateTime.Now.AddDays(-1).Date;

        foreach (var challenger in activeChallengers)
        {
            var yesterDayAttempt = challenger
                .DailyAttempts
                .FirstOrDefault(da => da.Date == yesterDay);

            if(yesterDayAttempt is null)
            {
                this.logger.LogWarning($"Failed to get daily attempts for username: {challenger.LeetcodeUserName}");
                continue;
            }

            messageBuilder.AppendLine(String.Format("| {0, -20} | {1, -6}| {2, -6}| {3, -6}|",
                        challenger.LeetcodeUserName,
                        challenger.Heart,
                        yesterDayAttempt.SolvedProblems,
                        challenger.TotalSolvedProblems)); 
        }

        return messageBuilder.ToString() + "</pre>";
    }

    private async Task RemoveMemberAsync(
        long groupId, 
        long memberId)
    {
        await telegramBotClient.BanChatMemberAsync(
            chatId: groupId,
            userId: memberId).ConfigureAwait(false);

        await telegramBotClient.SendTextMessageAsync(
            chatId: memberId,
            text: " Sizning imkoniyatlaringiz tugadi.\n" +
            "Guruhdan chetlatildingiz!");
    }
}
