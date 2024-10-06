namespace ZiziBot.Application.Handlers.Telegram.Text;

public class OcrBotRequest : BotRequestBase
{ }

public class OcrRequestHandler(
    ServiceFacade serviceFacade
) : IBotRequestHandler<OcrBotRequest>
{
    public async Task<BotResponseBase> Handle(OcrBotRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        await serviceFacade.TelegramService.SendMessageText("🔎 Sedang melakukan OCR");

        try
        {
            var localFile = await serviceFacade.TelegramService.DownloadFileAsync("ocr/");
            // var ocrResult = await _optiicDevService.ScanImageAsync(localFile);
            var result = await serviceFacade.OcrSpaceService.ParseImage(localFile);

            if (result.IsNullOrEmpty())
            {
                return await serviceFacade.TelegramService.EditMessageText("Sepertinya tidak ada teks di sana");
            }

            return await serviceFacade.TelegramService.EditMessageText(result);
        }
        catch (Exception exception)
        {
            var htmlMessage = HtmlMessage.Empty
                .Bold("Terjadi kesalahan ketika menjalankan OCR").Br()
                .Bold("Error: ").Text(exception.Message.Split(":").FirstOrDefault());

            return await serviceFacade.TelegramService.EditMessageText(htmlMessage.ToString());
        }
    }
}