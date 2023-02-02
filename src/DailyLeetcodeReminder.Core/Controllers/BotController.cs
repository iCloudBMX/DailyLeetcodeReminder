using DailyLeetcodeReminder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DailyLeetcodeReminder.Controllers;

[Route("bot")]
[ApiController]
public class BotController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] CommandHandler commandHandler)
    {
        await commandHandler
            .HandleCommandAsync(update);

        return Ok();
    }
}