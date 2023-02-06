using DailyLeetcodeReminder.Infrastructure.Models;
using System.Text;
using System.Text.Json;

namespace DailyLeetcodeReminder.Infrastructure.Services;

public class LeetCodeBroker : ILeetCodeBroker
{
    private readonly IHttpClientFactory httpClientFactory;

    public LeetCodeBroker(
        IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<UserProfile> GetUserProfile(string leetcodeUsername)
    {
        using (var httpClient = this.httpClientFactory
            .CreateClient("leetcode"))
        {
            var graphqlRequest = new GraphQLRequest
            {
                Query = @"
                    query ($username: String!) {
                        matchedUser(username: $username) {
                            username
                            submitStats {
                                acSubmissionNum {
                                    difficulty
                                    count
                                    submissions
                                }
                            }
                        }
                    }",
                Variables = new
                {
                    Username = leetcodeUsername
                }
            };

            var requestContent = new StringContent(
                content: JsonSerializer.Serialize(graphqlRequest),
                encoding: Encoding.UTF8,
                mediaType: "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(
                requestUri: string.Empty,
                content: requestContent);

            var contentString = await response.Content.ReadAsStringAsync();
            
            var leetcodeResult = JsonSerializer
                .Deserialize<LeetCodeResult>(contentString);

            return leetcodeResult.MatchedUser;
        }
    }
}