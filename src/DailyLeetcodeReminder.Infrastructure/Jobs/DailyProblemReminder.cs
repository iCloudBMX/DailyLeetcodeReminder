using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Quartz;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Infrastructure.Jobs
{
    public class DailyProblemReminder : IJob
    {
        private readonly IChallengerRepository challengerRepository;
        private readonly ILeetCodeBroker leetCodeBroker;
        private readonly ITelegramBotClient telegramBotClient;

        public DailyProblemReminder(
            IChallengerRepository challengerRepository,
            ILeetCodeBroker leetCodeBroker,
            ITelegramBotClient telegramBotClient)
        {
            this.challengerRepository = challengerRepository;
            this.leetCodeBroker = leetCodeBroker;
            this.telegramBotClient = telegramBotClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var challengersIds = await challengerRepository.SelectAllUsersTelegramIdAsync();
                var dailyTaskUrl = await leetCodeBroker.GetDailyChallengeUrlAsync();
                foreach (var challenger in challengersIds)
                {
                    await telegramBotClient.SendTextMessageAsync(
                        chatId: challenger,
                        text: $"You can solve on today's task through the following link \n " +
                        $"{dailyTaskUrl}");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
