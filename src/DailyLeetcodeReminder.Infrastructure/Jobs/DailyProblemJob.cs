using DailyLeetcodeReminder.Infrastructure.Brokers;
using DailyLeetcodeReminder.Infrastructure.Services;
using Quartz;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DailyLeetcodeReminder.Infrastructure.Jobs
{
    public class DailyProblemJob : IJob
    {
        private readonly ILeetCodeBroker leetCodeBroker;
        private readonly ITelegramBotClient telegramBotClient;

        public DailyProblemJob(
            ILeetCodeBroker leetCodeBroker,
            ITelegramBotClient telegramBotClient)
        {
            this.leetCodeBroker = leetCodeBroker;
            this.telegramBotClient = telegramBotClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DailyProblem dailyProblem =
                await leetCodeBroker.GetDailyProblemAsync();

            long groupId =
                long.Parse(Environment.GetEnvironmentVariable("GROUP_ID"));

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"<b>Daily problem</b> - {dailyProblem.Date}");
            builder.AppendLine($"<b>Problem</b> - <a href=\"https://leetcode.com{dailyProblem.Link}\">{dailyProblem.Title}</a>");
            builder.AppendLine($"<b>Difficulty</b> - {dailyProblem.Difficulty}");
            builder.AppendLine($"<b>Tags</b> - {dailyProblem.Tags}");

            var message = await telegramBotClient.SendTextMessageAsync(
                            chatId: groupId,
                            text: builder.ToString(),
                            parseMode: ParseMode.Html);

            await telegramBotClient.PinChatMessageAsync(groupId, message.MessageId);
        }
    }
}
