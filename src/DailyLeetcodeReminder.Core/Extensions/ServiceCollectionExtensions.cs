using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Core.Services;
using DailyLeetcodeReminder.Infrastructure.BackgroundServices;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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

        services.AddSingleton<ITelegramBotClient, TelegramBotClient>(x => new TelegramBotClient(botApiKey));

        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IChallengerService, ChallengerService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString: configuration.GetConnectionString("SqlServerConnectionString"),
                sqlServerOptionsAction: options =>
                {
                    options.EnableRetryOnFailure();
                });
        });

        services.AddScoped<IChallengerRepository, ChallengerRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();

        services.AddTransient<ILeetCodeBroker, LeetCodeBroker>();

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
        services
            .AddControllers()
            .AddNewtonsoftJson();

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

    public static IServiceCollection AddTimers(
        this IServiceCollection services)
    {
        int limitInMinutes = 1;

        services.AddSingleton<PeriodicTimer>(c =>
            new PeriodicTimer(TimeSpan.FromMinutes(limitInMinutes)));

        return services;
    }

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services)
    {
        services.AddHostedService<DailyReminderBackgroundService>();

        return services;
    }
}