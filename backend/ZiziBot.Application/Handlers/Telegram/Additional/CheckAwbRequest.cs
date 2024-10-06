namespace ZiziBot.Application.Handlers.Telegram.Additional;

public class CheckAwbRequest : BotRequestBase
{ }

public class CheckAwbHandler(
    ServiceFacade serviceFacade
)
    : IRequestHandler<CheckAwbRequest, BotResponseBase>
{
    public async Task<BotResponseBase> Handle(CheckAwbRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);
        var courier = request.MessageText.GetCommandParamAt<string>(0);
        var awb = request.MessageText.GetCommandParamAt<string>(1);

        if (request.MessageTexts?.Length > 2)
        {
            return serviceFacade.TelegramService.Complete();
        }

        if (courier == null || awb == null)
        {
            return await serviceFacade.TelegramService.SendMessageText("Masukkan no resi");
        }

        var check = await serviceFacade.TonjooService.GetAwbInfoMerged(courier, awb);
        if (check.Contains("tidak ada"))
        {
            check = await serviceFacade.BinderByteService.CekResiMergedAsync(courier, awb);
        }

        return await serviceFacade.TelegramService.SendMessageText(check);
    }
}