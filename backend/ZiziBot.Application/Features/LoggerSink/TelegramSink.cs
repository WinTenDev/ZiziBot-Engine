using AsyncAwaitBestPractices;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZiziBot.Application.Common.Types;
using IBatchedLogEventSink = Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink;

namespace ZiziBot.Application.Features.LoggerSink;

public class TelegramSink : ILogEventSink, IBatchedLogEventSink
{
    public string BotToken { get; init; }
    public long ChatId { get; init; }
    public int ThreadId { get; set; }
    private TelegramBotClient Bot => new TelegramBotClient(BotToken);
    private readonly ILogger _log = Log.ForContext<TelegramSink>();

    public void Emit(LogEvent logEvent)
    {
        EmitInternal(logEvent).SafeFireAndForget(e => _log.Error(e, "Error sending event log to Telegram"));
    }

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        foreach (var logEvent in batch)
        {
            await EmitInternal(logEvent);
        }
    }

    private async Task EmitInternal(LogEvent logEvent)
    {
        var exception = logEvent.Exception;

        if (ChatId == 0 || string.IsNullOrWhiteSpace(BotToken))
        {
            return;
        }

        var htmlMessage = HtmlMessage.Empty
            .CodeBr(logEvent.RenderMessage()).Br();

        htmlMessage.Text("Date: ").CodeBr(logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        foreach (var (key, value) in logEvent.Properties.Select(x => (x.Key, x.Value.ToString().Replace("\"", ""))))
        {
            htmlMessage.Bold($"{key}: ");

            if (value.IsValidUrl())
            {
                htmlMessage.TextBr(value);
            }
            else if (value.Contains('.'))
            {
                var end = value.Split(".").LastOrDefault();
                htmlMessage.CodeBr(end);
            }
            else
            {
                htmlMessage.CodeBr(value);
            }
        }

        if (exception != null)
        {
            htmlMessage.Bold("Message: ").CodeBr(exception.Message)
                .Bold("Source: ").CodeBr(exception.Source ?? "N/A")
                .Bold("Type: ").CodeBr(exception.GetType().Name);
        }

        var stackFrame = exception?.ToStackTrace()
            .GetFrames()
            .FirstOrDefault(frame => frame.GetFileLineNumber() > 0);

        if (stackFrame != null)
        {
            htmlMessage
                .Bold("File: ").CodeBr(stackFrame.GetFileName().GetFileName())
                .Bold("Coordinate: ").CodeBr($"{stackFrame.GetFileLineNumber()}:{stackFrame.GetFileColumnNumber()}")
                .Bold("Namespace: ").CodeBr(stackFrame.GetMethod()!.DeclaringType!.Namespace!)
                .Bold("Assembly: ").CodeBr(stackFrame.GetMethod()!.DeclaringType!.Assembly.GetName().Name ?? string.Empty)
                .Bold("Assembly Version: ").CodeBr(VersionUtil.GetVersion())
                .Bold("Assembly Location: ").CodeBr(stackFrame.GetMethod()!.DeclaringType!.Assembly.Location);
        }

        htmlMessage.Bold("#").Text(logEvent.Level.ToString()).Text(" ").Text($"#LOG_{logEvent.Timestamp:yyyyMMdd}");

        await SendMessageText(htmlMessage.ToString());
    }

    private async Task SendMessageText(string message)
    {
        var responseMessage = await Bot.SendMessage(
            chatId: ChatId,
            messageThreadId: ThreadId,
            parseMode: ParseMode.Html,
            text: message,
            linkPreviewOptions: new LinkPreviewOptions() {
                IsDisabled = true
            }
        );

        _log.Verbose("Event log to Telegram sent with MessageId: {MessageId}", responseMessage.MessageId);
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var logEvents = batch.ToList();
        _log.Debug("Preparing to send {Count} events to Telegram", logEvents.Count);

        foreach (var logEvent in logEvents)
        {
            await EmitInternal(logEvent);
        }

        _log.Debug("Sent {Count} events to Telegram", logEvents.Count);
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}

public static class TelegramSinkExtension
{
    public static LoggerSinkConfiguration Telegram(
        this LoggerSinkConfiguration configuration,
        string? botToken,
        long? chatId,
        int? threadId,
        LogEventLevel logEventLevel = LogEventLevel.Warning
    )
    {
        if (botToken == null || chatId == 0)
        {
            return configuration;
        }

        configuration.Sink(logEventSink: new TelegramSink() {
            BotToken = botToken,
            ChatId = chatId ?? 0,
            ThreadId = threadId ?? 0
        }, logEventLevel);

        return configuration;
    }

    public static LoggerSinkConfiguration TelegramBatched(
        this LoggerSinkConfiguration configuration,
        string? botToken,
        long? chatId,
        int? threadId,
        LogEventLevel logEventLevel = LogEventLevel.Warning
    )
    {
        if (botToken == null || chatId == 0)
        {
            return configuration;
        }

        var batchOptions = new PeriodicBatchingSinkOptions() {
            BatchSizeLimit = 100,
            Period = TimeSpan.FromSeconds(2)
        };

        var batchingSink = new PeriodicBatchingSink(new TelegramSink() {
            BotToken = botToken,
            ChatId = chatId ?? 0,
            ThreadId = threadId ?? 0
        }, batchOptions);

        configuration.Sink(batchingSink, logEventLevel);

        return configuration;
    }
}