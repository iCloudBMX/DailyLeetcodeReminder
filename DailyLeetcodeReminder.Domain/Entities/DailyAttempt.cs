namespace DailyLeetcodeReminder.Domain.Entities;

public class DailyAttempt
{
    public DateTime Date { get; set; }
    public long UserId { get; set; }
    public int SolvedProblems { get; set; }

    public Challenger User { get; }
}