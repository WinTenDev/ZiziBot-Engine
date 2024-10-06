using Allowed.Telegram.Bot.Controllers;
using Allowed.Telegram.Bot.Extensions.Collections;
using Microsoft.AspNetCore.Mvc;

namespace ZiziBot.Allowed.TelegramBot.Controllers;

[Route("telegram")]
[ApiExplorerSettings(IgnoreApi = true)]
public class TelegramController(
    IServiceProvider serviceProvider,
    ControllersCollection controllersCollection,
    ClientsCollection clientsCollection)
    : TelegramControllerBase(serviceProvider, controllersCollection, clientsCollection);