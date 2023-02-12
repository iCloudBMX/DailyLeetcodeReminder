namespace DailyLeetcodeReminder.Domain.Exceptions;

public class DuplicateException : Exception
{
  public DuplicateException(string leetcodeUserName) :
    base(message:"An attempt to re-register a Telegram or leetcode profile by a registered user"
    + $" LeetcodeUserName: {leetcodeUserName}")
  { }
}