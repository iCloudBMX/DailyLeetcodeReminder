using DailyLeetcodeReminder.Domain.Enums;

namespace DailyLeetcodeReminder.Domain.Entities;

public class Challenger
{
    public long TelegramId { get; set; }
    public string LeetcodeUserName { get; set; } = null!;
    public short Heart { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int TotalSolvedProblems { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<DailyAttempt> DailyAttempts { get; set; }
}