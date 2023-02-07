namespace DailyLeetcodeReminder.Domain.Entities;

public class ChallengerWithNoAttempt
{
    public long TelegramId { get; set; }
    public int TotalSolvedProblems { get; set; }
    public string LeetcodeUserName { get; set; }
}