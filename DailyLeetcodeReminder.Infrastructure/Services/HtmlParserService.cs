using HtmlAgilityPack;

namespace DailyLeetcodeReminder.Infrastructure.Services;

public class HtmlParserService : IHtmlParserService
{
    public int GetSolvedProblemsCount(string html)
    {
        string className = "text-[24px] font-medium text-label-1 dark:text-dark-label-1";

        string solvedProblemsCountText = GetDivInnerTextByClassName(html, className);

        if(int.TryParse(solvedProblemsCountText, out int solvedProblemsCount))
        {
            return solvedProblemsCount;
        }

        throw new FormatException($"Couldn't parse element. Value is: {solvedProblemsCountText}");
    }

    private string GetDivInnerTextByClassName(string html, string className)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var div = htmlDoc.DocumentNode.Descendants("div")
            .FirstOrDefault(d => d.Attributes.Contains("class")
                && d.Attributes["class"].Value.Contains(className));

        if(div is null)
        {
            return string.Empty;
        }

        return div.InnerText.Trim();
    }
}