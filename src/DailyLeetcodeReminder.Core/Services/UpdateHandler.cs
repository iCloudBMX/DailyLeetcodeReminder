using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Domain.Entities;
using DailyLeetcodeReminder.Domain.Enums;
using DailyLeetcodeReminder.Domain.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace DailyLeetcodeReminder.Core.Services;

public class UpdateHandler
{
    private readonly IChallengerService challengerService;
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ILogger<UpdateHandler> logger;
    private static int pageSize = 10;

    public UpdateHandler(
        IChallengerService challengerService,
        ITelegramBotClient telegramBotClient,
        ILogger<UpdateHandler> logger)
    {
        this.challengerService = challengerService;
        this.telegramBotClient = telegramBotClient;
        this.logger = logger;
    }

    public async Task UpdateHandlerAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => HandleCommandAsync(update.Message),
            UpdateType.CallbackQuery => OnCallbackQueryReceivedAsync(update.CallbackQuery),
            _ => HandleNotAvailableCommandAsync(update.Message)
        };

        await handler;
    }

    private async Task OnCallbackQueryReceivedAsync(CallbackQuery callbackQuery)
    {
        var challengers = await challengerService.RetrieveChallengers();

        int page = int.Parse(callbackQuery.Data);

        var sortedChallengers = challengers.OrderByDescending(ch => ch.TotalSolvedProblems)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();

        await telegramBotClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: $"<b>{ServiceHelper.TableBuilder(sortedChallengers)}</b>",
            replyMarkup: ServiceHelper.GenerateButtons(challengers.Count / pageSize),
            parseMode: ParseMode.Html);
    }

    public async Task HandleCommandAsync(Message message)
    {
        if (!message.Text.StartsWith("/"))
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
                "rank" => HandleRankCommandAsync(message),
                _ => HandleNotAvailableCommandAsync(message)
            };

            await task;
        }
        catch (AlreadyExistsException exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Siz allaqachon ro'yxatdan o'tgansiz");

            return;
        }
        catch (NotFoundException exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Kechirasiz usernameni tekshirib qayta urining, username topilmadi");

            return;
        }
        catch (DuplicateException exception)
        {
            this.logger.LogError(exception.Message);

            await this.telegramBotClient.SendTextMessageAsync(
                chatId: message.From.Id,
                text: "Sizning telegram yoki leetcode profilingiz ro'yxatdan o'tgan");

            return;
        }
        catch (Exception exception)
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

    private async Task HandleRankCommandAsync(Message message)
    {
        var challengers = await challengerService.RetrieveChallengers();
        
        var sortedChallengers = challengers
        .OrderByDescending(ch => ch.TotalSolvedProblems)
        .Skip(0).Take(10).ToList();

        await telegramBotClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"<b>{ServiceHelper.TableBuilder(sortedChallengers)}</b>",
            replyMarkup: ServiceHelper.GenerateButtons(challengers.Count / 10),
            parseMode: ParseMode.Html);
    }
}
