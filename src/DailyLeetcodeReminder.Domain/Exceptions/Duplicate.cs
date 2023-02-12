namespace DailyLeetcodeReminder.Domain.Exceptions;

public class DuplicateException : Exception
{
  public DuplicateException(long telegramId,string leetcodeUserName) :
    base(message:"An attempt to re-register a Telegram or leetcode profile by a registered user"
    + $" TelegramId: {telegramId}, LeetcodeUserName: {leetcodeUserName}")
  { }
}