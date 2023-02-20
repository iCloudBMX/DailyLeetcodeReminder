using DailyLeetcodeReminder.Application.Services;
using DailyLeetcodeReminder.Core.Services;
using DailyLeetcodeReminder.Infrastructure.Jobs;
using DailyLeetcodeReminder.Infrastructure.Contexts;
using DailyLeetcodeReminder.Infrastructure.Repositories;
using DailyLeetcodeReminder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;
using Npgsql;

namespace DailyLeetcodeReminder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBotClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        string botApiKey = Environment.GetEnvironmentVariable("BOT_API_KEY");

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
            string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            var databaseUri = new Uri(connectionString);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/')
            };

            options.UseNpgsql(connectionString: builder.ToString());
        });

        services.AddScoped<IChallengerRepository, ChallengerRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();

        services.AddTransient<ILeetCodeBroker, LeetCodeBroker>();

        return services;
    }

    public static IServiceCollection AddUpdateHandler(
        this IServiceCollection services)
    {
        services.AddTransient<UpdateHandler>();

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

    public static IServiceCollection AddJobs(
        this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionScopedJobFactory();

            var dailyReminderJobKey = new JobKey(nameof(DailyReminderJob));

            q.AddJob<DailyReminderJob>(opts =>
            {
                opts.WithIdentity(dailyReminderJobKey);
            });

            q.AddTrigger(opts => opts
                .ForJob(dailyReminderJobKey)
                .WithIdentity($"{dailyReminderJobKey.Name}-trigger")
                .WithCronSchedule("0 0 9,14,23 * * ?")
            );

            var dailyReportJobKey = new JobKey(nameof(DailyReportJob));
            
            q.AddJob<DailyReportJob>(opts =>
            {
                opts.WithIdentity(dailyReportJobKey);
            });

            q.AddTrigger(opts => opts
                .ForJob(dailyReportJobKey)
                .WithIdentity($"{dailyReportJobKey.Name}-trigger")
                .WithCronSchedule("0 54 0 * * ?")
            );

            var dailyProblemJobKey = new JobKey(nameof(DailyProblemJob));

            q.AddJob<DailyProblemJob>(opts =>
            {
                opts.WithIdentity(dailyProblemJobKey);
            });

            q.AddTrigger(opts => opts
                .ForJob(dailyProblemJobKey)
                .WithIdentity($"{dailyProblemJobKey.Name}-trigger")
                .WithCronSchedule("0 0 13 * * ?")
            );
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}