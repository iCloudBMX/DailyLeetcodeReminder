using Newtonsoft.Json;

namespace DailyLeetcodeReminder.Infrastructure.Models;

public class Submission
{
    [JsonProperty("difficulty")]
    public string Difficulty { get; set; }
    
    [JsonProperty("count")]
    public int Count { get; set; }
    
    [JsonProperty("submissions")]
    public int Submissions { get; set; }
}