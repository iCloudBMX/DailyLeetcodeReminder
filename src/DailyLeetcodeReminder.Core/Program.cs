using DailyLeetcodeReminder.Core.Extensions;
using DailyLeetcodeReminder.Core.Services;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Requests;

namespace DailyLeetcodeReminder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddApplication()
                .AddInfrastructure()
                .AddComamndHandler()
                .AddTelegramBotClient(builder.Configuration)
                .AddSwagger()
                .AddControllerMappers()
                .AddHttpClientServices(builder.Configuration);             

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

            SetWebHook(app, builder.Configuration);
        }

        private static void SetWebHook(
            IApplicationBuilder builder,
            IConfiguration configuration)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var baseUrl = configuration.GetSection("TelegramBot:BaseUrl").Value;
                var webhookUrl = $"{baseUrl}/api/bot";
                botClient.SetWebhookAsync(webhookUrl).Wait();
            }
        }
    }
}