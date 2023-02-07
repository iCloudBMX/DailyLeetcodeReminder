using System.ComponentModel.DataAnnotations.Schema;

namespace DailyLeetcodeReminder.Domain.Entities;

[NotMapped]
public class ChallengerWithNoAttempt
{
    public long TelegramId { get; set; }
    public int TotalSolvedProblems { get; set; }
    public string LeetcodeUserName { get; set; }
}