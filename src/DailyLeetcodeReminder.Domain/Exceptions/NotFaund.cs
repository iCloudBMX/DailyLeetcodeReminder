namespace DailyLeetcodeReminder.Domain.Exceptions;

public class NotFoundException : Exception
{
  public NotFoundException():
    base(message:"Leetcode username not found")
  {
    
  }
}