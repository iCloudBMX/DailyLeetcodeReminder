using DailyLeetcodeReminder.Core.Extensions;
using DailyLeetcodeReminder.Core.Middlewares;
using Telegram.Bot;

namespace DailyLeetcodeReminder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services
                .AddApplication()
                .AddInfrastructure(builder.Configuration)
                .AddUpdateHandler()
                .AddTelegramBotClient(builder.Configuration)
                .AddSwagger()
                .AddControllerMappers()
                .AddHttpClientServices(builder.Configuration)
                .AddJobs();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            app.MapControllers();
            
            SetWebHook(app, builder.Configuration);
         
            app.Run();
        }

        private static void SetWebHook(
            IApplicationBuilder builder,
            IConfiguration configuration)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var baseUrl = Environment.GetEnvironmentVariable("BASE_ADDRESS");
                var webhookUrl = $"{baseUrl}/bot";
                var webhookInfo = botClient.GetWebhookInfoAsync().Result;

                if (webhookInfo is null || webhookInfo.Url != webhookUrl)
                {
                    botClient.SetWebhookAsync(webhookUrl).Wait();
                }
            }
        }
    }
}