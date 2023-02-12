namespace DailyLeetcodeReminder.Domain.Exceptions;

public class AlreadyExistsException : Exception
{
  public AlreadyExistsException(string leetcodeUserName) :
    base(message:"A registered user tried to register again"
    + $" LeetcodeUserName: {leetcodeUserName}")
  { }
}