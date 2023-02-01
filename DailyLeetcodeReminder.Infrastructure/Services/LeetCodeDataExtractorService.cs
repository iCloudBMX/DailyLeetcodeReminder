namespace DailyLeetcodeReminder.Infrastructure.Services;

public class LeetCodeDataExtractorService : ILeetCodeDataExtractorService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IHtmlParserService htmlParserService;

    public LeetCodeDataExtractorService(
        IHttpClientFactory httpClientFactory,
        IHtmlParserService htmlParserService)
    {
        this.httpClientFactory = httpClientFactory;
        this.htmlParserService = htmlParserService;
    }

    public async Task<int> ExtractSolvedProblemsCountAsync(
        string leetcodeUsername)
    {
        using(var httpClient = this.httpClientFactory
            .CreateClient("leetcode"))
        {
            var content = await httpClient
                .GetStringAsync(leetcodeUsername);

            int solvedProblemCount = this.htmlParserService
                .GetSolvedProblemsCount(content);

            return solvedProblemCount;
        }
    }
}