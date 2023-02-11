namespace DailyLeetcodeReminder.Domain.Exceptions;

public class NotFaundLeetCodeUserNameException : Exception
{
  public NotFaundLeetCodeUserNameException():
    base(message:"Kechirasiz usernameni tekshirib qayta urining, username topilmadi")
  {
    
  }
}