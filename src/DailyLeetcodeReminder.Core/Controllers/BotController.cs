using DailyLeetcodeReminder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace DailyLeetcodeReminder.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly CommandHandler commandHandler;

    public BotController(CommandHandler commandHandler)
    {
        this.commandHandler = commandHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        await this.commandHandler.HandleCommandAsync(update);

        return Ok();
    }
}