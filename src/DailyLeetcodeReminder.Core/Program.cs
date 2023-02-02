using DailyLeetcodeReminder.Core.Extensions;
using Telegram.Bot;

namespace DailyLeetcodeReminder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddApplication()
                .AddInfrastructure(builder.Configuration)
                .AddComamndHandler()
                .AddTelegramBotClient(builder.Configuration)
                .AddSwagger()
                .AddControllerMappers()
                .AddHttpClientServices(builder.Configuration);             

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();
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
                var baseUrl = configuration.GetSection("TelegramBot:BaseAddress").Value;
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