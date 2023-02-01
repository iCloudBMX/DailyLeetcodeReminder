using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Core.Services;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Telegram.Bot;

namespace DailyLeetcodeReminder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBotClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        string botApiKey = configuration
            .GetSection("TelegramBot:ApiKey").Value;

        services.AddSingleton(x => new TelegramBotClient(botApiKey));

        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IChallengerService, ChallengerService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IChallengerRepository, ChallengerRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();

        services.AddTransient<ILeetCodeDataExtractorService, LeetCodeDataExtractorService>();
        services.AddTransient<IHtmlParserService, HtmlParserService>();

        return services;
    }

    public static IServiceCollection AddComamndHandler(
        this IServiceCollection services)
    {
        services.AddTransient<CommandHandler>();

        return services;
    }

    public static IServiceCollection AddSwagger(
        this IServiceCollection services)
    {
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddControllerMappers(
        this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        return services;
    }

    public static IServiceCollection AddHttpClientServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("leetcode", config =>
        {
            var baseAddress = configuration
                .GetSection("Leetcode:BaseAddress").Value;

            config.BaseAddress = new Uri(baseAddress);
        });

        return services;
    }
}