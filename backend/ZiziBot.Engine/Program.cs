using Serilog;
using ZiziBot.Application.Infrastructure.Extensions;
using ZiziBot.Application.Infrastructure.Database.Extensions;
using ZiziBot.Presentation.Bots.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadSettings();

builder.WebHost.ConfigureCustomListenPort();

builder.Services.ConfigureServices();
builder.Services.ConfigureTelegramBot();

builder.Services.AddSerilog(builder);
builder.Services.ConfigureScheduler(builder.Configuration);
builder.Services.AddRestApi();
builder.Services.AddAllMiddleware();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.ConfigureFlurl();

await app.PrintAbout();
await app.UseMongoMigration();
await app.PrefetchRepository();

app.UseAuthorization();

app.ConfigureApi();
app.UseScheduler();

await app.RunTelegramBot();

await app.RunAsync();
