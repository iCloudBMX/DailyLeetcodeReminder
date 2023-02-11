namespace DailyLeetcodeReminder.Domain.Exceptions;

public class AlreadyExictException : Exception
{
  public AlreadyExictException() :
    base(message:"Siz allaqachon ro'yxatdan o'tgansiz")
  { }
}