using Newtonsoft.Json;

namespace DailyLeetcodeReminder.Infrastructure.Models;


public class SubmissionNumber
{
    public string Difficulty { get; set; }
    public int Count { get; set; }
    public int Submissions { get; set; }
}

public class SubmitStatistics
{
    public List<SubmissionNumber> Submissions { get; set; }
}

public class UserProfile
{
    public string Username { get; set; }

    [JsonProperty("submitStats")]
    public SubmitStatistics SubmitStatistics { get; set; }
}

public class LeetCodeResult
{
    [JsonProperty("matchedUser")]
    public UserProfile MatchedUser { get; set; }
}