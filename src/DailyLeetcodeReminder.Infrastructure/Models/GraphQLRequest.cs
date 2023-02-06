namespace DailyLeetcodeReminder.Infrastructure.Models;

public class GraphQLRequest
{
    public string Query { get; set; }
    public object Variables { get; set; }
}