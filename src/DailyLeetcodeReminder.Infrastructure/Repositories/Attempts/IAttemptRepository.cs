namespace DailyLeetcodeReminder.Infrastructure.Repositories;

public interface IAttemptRepository
{
    Task MarkDailyAttemptsAsync(List<long> challengerIds);
}