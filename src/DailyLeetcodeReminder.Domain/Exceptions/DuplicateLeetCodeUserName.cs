namespace DailyLeetcodeReminder.Domain.Exceptions;

public class DuplicateLeetCodeUserNameException : Exception
{
  public DuplicateLeetCodeUserNameException():
    base(message:"Bu LeetCode username ro'yxatga olinga")
  { }
}