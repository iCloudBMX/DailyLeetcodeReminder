﻿using DailyLeetcodeReminder.Infrastructure.Brokers.LeetCode;
using DailyLeetcodeReminder.Infrastructure.Models;
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
            
            builder.AppendLine($"<b>Kun masalasi</b> - {dailyProblem.Date}");
            builder.AppendLine($"<b>Masala</b> - <a href=\"https://leetcode.com{dailyProblem.Link}\">{dailyProblem.Title}</a>");
            builder.AppendLine($"<b>Qiyinchilik</b> - {dailyProblem.Difficulty}");
            builder.AppendLine($"<b>Teglar</b> - {dailyProblem.Tags}");

            var message = await telegramBotClient.SendTextMessageAsync(
                chatId: groupId,
                text: builder.ToString(),
                parseMode: ParseMode.Html);

            await telegramBotClient.PinChatMessageAsync(groupId, message.MessageId);
        }
    }
}
