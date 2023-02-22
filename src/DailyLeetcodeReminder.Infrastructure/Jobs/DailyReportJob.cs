using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Infrastructure.Brokers.DateTimes;
using DailyLeetcodeReminder.Infrastructure.Brokers.LeetCode;
using DailyLeetcodeReminder.Infrastructure.Repositories;
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
    private readonly IDateTimeBroker dateTimeBroker;

    public DailyReportJob(
        IChallengerRepository challengerRepository,
        ITelegramBotClient telegramBotClient,
        ILeetCodeBroker leetcodeBroker,
        ILogger<DailyReportJob> logger,
        IDateTimeBroker dateTimeBroker)
    {
        this.challengerRepository = challengerRepository;
        this.telegramBotClient = telegramBotClient;
        this.leetcodeBroker = leetcodeBroker;
        this.logger = logger;
        this.dateTimeBroker = dateTimeBroker;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        long groupId = long.Parse(Environment.GetEnvironmentVariable("GROUP_ID"));
        var today = dateTimeBroker.GetCurrentDateTime().Date;
        var yesterday = today.AddDays(-1);

        List<Challenger> activeChallengers = await challengerRepository
            .SelectActiveChallengersWithAttemptsAsync();

        foreach (var activeChallenger in activeChallengers)
        {
            var totalSolvedProblemsCount = await leetcodeBroker
                .GetTotalSolvedProblemsCountAsync(activeChallenger.LeetcodeUserName);

            int yesterdayProblemsSolved = totalSolvedProblemsCount - activeChallenger.TotalSolvedProblems;

            if (yesterdayProblemsSolved == 0 &&
                activeChallenger.CreatedAt.Date < yesterday)
            {
                activeChallenger.Heart--;
            }

            if (activeChallenger.Heart <= 0)
            {
                activeChallenger.Status = UserStatus.Inactive;

                var chatMember = await telegramBotClient
                    .GetChatMemberAsync(groupId, activeChallenger.TelegramId);

                if (chatMember.Status is not ChatMemberStatus.Administrator and not ChatMemberStatus.Creator)
                {
                    await telegramBotClient.BanChatMemberAsync(
                        chatId: groupId,
                        userId: activeChallenger.TelegramId).ConfigureAwait(false);
                }
            }
            else
            {
                activeChallenger.TotalSolvedProblems = totalSolvedProblemsCount;
            }

            var yesterdayAttempt = activeChallenger
                .DailyAttempts
                .FirstOrDefault(da => da.Date == yesterday);

            if (yesterdayAttempt is not null)
            {
                yesterdayAttempt.SolvedProblems = yesterdayProblemsSolved;
            }


            activeChallenger.DailyAttempts.Add(new DailyAttempt
            {
                UserId = activeChallenger.TelegramId,
                Date = today,
                SolvedProblems = 0
            });
        }

        await challengerRepository.SaveChangesAsync();

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
        DateTime currentDay = dateTimeBroker.GetCurrentDateTime();

        messageBuilder.AppendLine($"Hisobot - <b>{currentDay.ToString("dd.MM.yyyy")}</b>\n");

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

        var yesterday = currentDay.AddDays(-1).Date;

        foreach (var challenger in activeChallengers)
        {
            var yesterdayAttempt = challenger
                .DailyAttempts
                .FirstOrDefault(da => da.Date == yesterday);

            if (yesterdayAttempt is null)
            {
                this.logger.LogWarning($"Failed to get daily attempts for username: {challenger.LeetcodeUserName}");
                continue;
            }

            messageBuilder.AppendLine(String.Format("| {0, -20} | {1, -6}| {2, -6}| {3, -6}|",
                        challenger.LeetcodeUserName,
                        challenger.Heart,
                        yesterdayAttempt.SolvedProblems,
                        challenger.TotalSolvedProblems));
        }

        return messageBuilder.ToString() + "</pre>";
    }
}
