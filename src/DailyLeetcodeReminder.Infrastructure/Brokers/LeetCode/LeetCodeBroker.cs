using DailyLeetcodeReminder.Domain.Exceptions;
using DailyLeetcodeReminder.Infrastructure.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DailyLeetcodeReminder.Infrastructure.Brokers.LeetCode;

public class LeetCodeBroker : ILeetCodeBroker
{
    private readonly IHttpClientFactory httpClientFactory;

    public LeetCodeBroker(
        IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<DailyProblem> GetDailyProblemAsync()
    {
        using (var httpClient = httpClientFactory
            .CreateClient("leetcode"))
        {
            var graphqlRequest = new GraphQLRequest
            {
                Query = @"
                    query questionOfToday {
                        activeDailyCodingChallengeQuestion {
                            date
                            link
                            question{
                                difficulty
                                title
                                topicTags {
                                 name
                                }
                            }
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
            var jsonObject = JsonNode.Parse(contentString);
            var dailyProblem = new DailyProblem();
            MapToDailyProblem(jsonObject, dailyProblem);
            CheckLink(dailyProblem);

            return dailyProblem;
        }
    }

    public async Task<int> GetTotalSolvedProblemsCountAsync(string leetcodeUsername)
    {
        using (var httpClient = httpClientFactory
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
            var jsonObject = JsonNode.Parse(contentString);

            if (jsonObject?["errors"] is not null)
            {
                throw new NotFoundException(leetcodeUsername);
            }

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
    private static void MapToDailyProblem(JsonNode? jsonObject, DailyProblem dailyProblem)
    {
        dailyProblem.Link = jsonObject?["data"]?
                                       ["activeDailyCodingChallengeQuestion"]?
                                       ["link"]
                                       .GetValue<string>();

        dailyProblem.Difficulty = jsonObject?["data"]?
                                             ["activeDailyCodingChallengeQuestion"]?
                                             ["question"]
                                             ["difficulty"]
                                             .GetValue<string>();

        dailyProblem.Title = jsonObject?["data"]?
                                        ["activeDailyCodingChallengeQuestion"]?
                                        ["question"]
                                        ["title"]
                                        .GetValue<string>();

        var tagsList = jsonObject?["data"]?
                                  ["activeDailyCodingChallengeQuestion"]?
                                  ["question"]
                                  ["topicTags"];

        dailyProblem.Tags = string.Join(", ", tagsList.AsArray().Select(tag => tag["name"]));

        dailyProblem.Date = jsonObject?["data"]?
                                       ["activeDailyCodingChallengeQuestion"]?
                                       ["date"]
                                       .GetValue<string>();

    }
    private static void CheckLink(DailyProblem dailyProblem)
    {
        if (string.IsNullOrEmpty(dailyProblem.Link))
        {
            throw new Exception("Failed to retrieve daily challenge url");
        }
    }

}