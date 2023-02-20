using DailyLeetcodeReminder.Domain.Entities;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace DailyLeetcodeReminder.Core.Services;

public static class ServiceHelper
{
    public static InlineKeyboardMarkup GenerateButtons(int page)
    {
        var buttons = new List<InlineKeyboardButton>()
        {
            new InlineKeyboardButton("⏪")
            {
                CallbackData = $"prev {page}"
            },
            new InlineKeyboardButton("⏩")
            {
                CallbackData = $"next {page}"
            }
        };

        return new InlineKeyboardMarkup(buttons);
    }

    public static string TableBuilder(List<Challenger> challengers)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Reyting");
        builder.AppendLine($"<pre>|{new string('-', 22)}|{new string('-', 10)}|");

        builder.AppendLine(String.Format("| {0, -20} | {1, -9}|",
            "Foydalanuvchi nomi", "Jami"));

        builder.AppendLine($"|{new string('-', 22)}|{new string('-', 10)}|");

        foreach (var challanger in challengers)
        {
            builder.AppendLine(String.Format("| {0, -20} | {1, -9}|",
                challanger.LeetcodeUserName, challanger.TotalSolvedProblems));
        }

        return builder.ToString() + "</pre>";
    }
}
