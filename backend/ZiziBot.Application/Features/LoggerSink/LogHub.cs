using Microsoft.AspNetCore.SignalR;
using IHub = Serilog.Sinks.AspNetCore.SignalR.Interfaces.IHub;

namespace ZiziBot.Application.Features.LoggerSink;

public class LogHub : Hub<IHub>
{

}