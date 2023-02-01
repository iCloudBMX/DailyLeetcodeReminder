using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Domain.Enums;
using Telegram.Bot.Types;
using DailyLeetcodeReminder.Domain.Entities;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Core.Services;

public class CommandHandler
{
    private readonly IChallengerService challengerService;
    private readonly ITelegramBotClient telegramBotClient;

    public CommandHandler(
        IChallengerService challengerService,
        ITelegramBotClient telegramBotClient)
    {
        this.challengerService = challengerService;
        this.telegramBotClient = telegramBotClient;
    }

    public async Task HandleCommandAsync(Update update)
    {
        var message = update.Message;

        if (message == null || !message.Text.StartsWith("/"))
        {
            return;
        }

        var command = message.Text.Split(' ').First().Substring(1);
        
        switch (command)
        {
            case "register":
                await HandleRegisterCommandAsync(message);
                break;
            
            // handle other commands here
        }
    }

    private async Task HandleRegisterCommandAsync(Message message)
    {
        var leetCodeUsername = message.Text?.Split(' ').Skip(1).FirstOrDefault();
        
        if (string.IsNullOrWhiteSpace(leetCodeUsername))
        {
            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Please provide your leetcode platform username after /register command. Like: /register myusername");

            return;
        }

        var challenger = new Challenger
        {
            TelegramId = message.From.Id,
            LeetcodeUserName = leetCodeUsername,
            FirstName = message.From.FirstName,
            LastName = message.From.LastName,
            Status = UserStatus.Active,
        };

        Challenger insertedChallenger = await this.challengerService
            .AddUserAsync(challenger);

        await this.telegramBotClient.SendTextMessageAsync(
            chatId: insertedChallenger.TelegramId,
            text: "You have successfully registered");
    }
}