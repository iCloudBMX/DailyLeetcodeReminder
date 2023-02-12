using DailyLeetcodeReminder.Domain.Exceptions;
using DailyLeetcodeReminder.Infrastructure.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DailyLeetcodeReminder.Infrastructure.Services;

public class LeetCodeBroker : ILeetCodeBroker
{
    private readonly IHttpClientFactory httpClientFactory;

    public LeetCodeBroker(
        IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetDailyChallengeUrlAsync()
    {
        using (var httpClient = this.httpClientFactory
            .CreateClient("leetcode"))
        {
            var graphqlRequest = new GraphQLRequest
            {
                Query = @"
                    query questionOfToday {
                        activeDailyCodingChallengeQuestion {
                            link
                        }
                    }"
            };

            var requestContent = new StringContent(
                content: JsonSerializer.Serialize(
                    value: graphqlRequest,
                    options: new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                encoding: Encoding.UTF8,
                mediaType: "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(
                requestUri: string.Empty,
                content: requestContent);

            var contentString = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonObject.Parse(contentString);
            
            if(jsonObject?["errors"] is not null)
            {
                throw new NotFoundException();
            }

            string? dailyChallengeUrl = jsonObject?["data"]?["activeDailyCodingChallengeQuestion"]["link"]
                .GetValue<string>();

            if (string.IsNullOrEmpty(dailyChallengeUrl))
            {
                throw new Exception("Failed to retrieve daily challenge url");
            }

            return dailyChallengeUrl;
        }
    }

    public async Task<int> GetTotalSolvedProblemsCountAsync(string leetcodeUsername)
    {
        using (var httpClient = this.httpClientFactory
            .CreateClient("leetcode"))
        {
            var graphqlRequest = new GraphQLRequest
            {
                Query = @"
                    query ($username: String!) {
                        matchedUser(username: $username) {
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
                content: JsonSerializer.Serialize(
                    value: graphqlRequest,
                    options: new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                encoding: Encoding.UTF8,
                mediaType: "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(
                requestUri: string.Empty,
                content: requestContent);

            var contentString = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonObject.Parse(contentString);

            int? totalSolvedProblemsCount = jsonObject?["data"]?["matchedUser"]?["submitStats"]["acSubmissionNum"]
                .Deserialize<List<Submission>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                .Where(submission => submission.Difficulty == "All")
                .Select(submission => submission.Count)
                .FirstOrDefault();

            if (totalSolvedProblemsCount.HasValue is false)
            {
                throw new FormatException("Failed to retrieve solved problems count");
            }

            return totalSolvedProblemsCount.Value;
        }
    }
}