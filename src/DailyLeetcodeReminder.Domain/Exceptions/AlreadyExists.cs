namespace DailyLeetcodeReminder.Domain.Exceptions;

public class AlreadyExistsException : Exception
{
  public AlreadyExistsException(long telegramId,string leetcodeUserName) :
    base(message:"A registered user tried to register again"
    + $" TelegramId: {telegramId}, LeetcodeUserName: {leetcodeUserName}")
  { }
}