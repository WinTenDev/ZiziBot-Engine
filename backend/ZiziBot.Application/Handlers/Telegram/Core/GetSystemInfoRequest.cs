using System.Diagnostics;
using System.Reflection;
using Humanizer;
using Environment = System.Environment;

namespace ZiziBot.Application.Handlers.Telegram.Core;

public class GetSystemInfoRequest : BotRequestBase
{ }

public class GetSystemInfoHandler(
    ServiceFacade serviceFacade
)
    : IBotRequestHandler<GetSystemInfoRequest>
{
    public async Task<BotResponseBase> Handle(GetSystemInfoRequest request, CancellationToken cancellationToken)
    {
        serviceFacade.TelegramService.SetupResponse(request);

        var workingSet = Process.GetCurrentProcess().WorkingSet64.Bytes().ToString("#.##");
        var privateMemory = Process.GetCurrentProcess().PrivateMemorySize64.Bytes().ToString("#.##");
        var totalMemory = GC.GetTotalMemory(true).Bytes().ToString("#.##");

        var htmlMessage = HtmlMessage.Empty
            .Bold("🏠 Host Information").Br()
            .Bold("OS: ").Code(Environment.OSVersion.ToString()).Br()
            .Bold("HostName: ").Code(Environment.MachineName).Br()
            .Bold(".NET Version: ").Code(Environment.Version.ToString()).Br()
            .Br()
            .Bold("💻 System Information").Br()
            .Bold("CPU: ").Code(Environment.ProcessorCount + " CPUs").Br()
            .Bold("WS: ").Code(workingSet).Br()
            .Bold("PM: ").Code(privateMemory).Br()
            .Bold("GC: ").Code(totalMemory).Br()
            .Br()
            .Bold("🤖 Assembly Information").Br()
            .Bold("Version: ").Code(Assembly.GetEntryAssembly()!.GetName().Version!.ToString()).Br()
            .Bold("FullName: ").Code(Assembly.GetEntryAssembly()!.GetName().Name!).Br()
            .Bold("Executable: ").Code(Environment.ProcessPath!).Br();


        return await serviceFacade.TelegramService.SendMessageAsync(htmlMessage.ToString());
    }
}