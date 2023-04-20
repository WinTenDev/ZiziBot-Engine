using Telegram.Bot;

namespace ZiziBot.Application.Handlers.Telegram.Basic;

public class GetAboutRequest : RequestBase
{
}

public class GetAboutHandler : IRequestHandler<GetAboutRequest, ResponseBase>
{
    private readonly TelegramService _telegramService;
    private readonly AppSettingRepository _appSettingRepository;

    public GetAboutHandler(TelegramService telegramService, AppSettingRepository appSettingRepository)
    {
        _telegramService = telegramService;
        _appSettingRepository = appSettingRepository;
    }

    public async Task<ResponseBase> Handle(GetAboutRequest request, CancellationToken cancellationToken)
    {
        var htmlMessage = HtmlMessage.Empty;
        _telegramService.SetupResponse(request);

        var me = await _telegramService.Bot.GetMeAsync(cancellationToken: cancellationToken);
        var config = await _appSettingRepository.GetConfigSection<EngineConfig>();

        if (config != null)
        {
            htmlMessage
                .Bold(config.ProductName).Br()
                .Text("by ").Text(config.Vendor).Br()
                .Text(config.Description).Br().Br();
        }

        htmlMessage
            .Bold("Version: ").Text(VersionUtil.GetVersion(true)).Text($" (").Code(VersionUtil.GetVersion()).Text(")").Br()
            .Bold("Build Date: ").Code(VersionUtil.GetBuildDate().ToString("u")).Br();

        return await _telegramService.SendMessageText(htmlMessage.ToString());
    }
}