namespace DailyLeetcodeReminder.Domain.Exceptions;

public class NotFoundException : Exception
{
  public NotFoundException(string leetcodeUsername):
    base(message:"Leetcode username not found"
    + $"LeetcodeUserName: {leetcodeUsername}")
  {
    
  }
}