using DailyLeetcodeReminder.Infrastructure.Models;

namespace DailyLeetcodeReminder.Infrastructure.Services;

public interface ILeetCodeBroker
{
    Task<UserProfile> GetUserProfile(string leetcodeUsername);
}