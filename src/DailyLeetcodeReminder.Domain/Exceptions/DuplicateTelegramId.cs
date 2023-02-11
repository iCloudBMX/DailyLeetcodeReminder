namespace DailyLeetcodeReminder.Domain.Exceptions;

public class DuplicateTelegramIdException : Exception
{
  public DuplicateTelegramIdException() :
    base(message:"Sizning telegram profiliz ro'yxatdan o'tgan")
  { }
}