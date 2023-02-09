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
    private readonly ILogger<CommandHandler> logger;

    public CommandHandler(
        IChallengerService challengerService,
        ITelegramBotClient telegramBotClient,
        ILogger<CommandHandler> logger)
    {
        this.challengerService = challengerService;
        this.telegramBotClient = telegramBotClient;
        this.logger = logger;
    }

    public async Task HandleCommandAsync(Update update)
    {

        var message = update.Message;

        if (message == null || !message.Text.StartsWith("/"))
        {
            return;
        }

        var command = message.Text.Split(' ').First().Substring(1);

        try
        {
            var task = command switch
            {
                "start" => HandleStartCommandAsync(message),
                "register" => HandleRegisterCommandAsync(message),
                _ => HandleNotAvailableCommandAsync(message)
            };

            await task;
        }
        catch(Exception exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Failed to handle your request. Please try again");

            return;
        }
    }

    private async Task HandleStartCommandAsync(Message message)
    {
        await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Daily leetcode botiga xush kelibsiz. " +
                "Kunlik challenge'da qatnashish uchun, " +
                "leetcode username'ni /register komandasidan keyin yuboring. " +
                "Misol uchun: /register username");
    }

    private async Task HandleNotAvailableCommandAsync(Message message)
    {
        await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Mavjud bo'lmagan komanda kiritildi. " +
                "Tekshirib ko'ring.");
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