using Cortex.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ZiziBot.Application.Infrastructure.Extensions;

public static class CortexExtension
{
    internal static IServiceCollection AddApplicationCortexMediator(this IServiceCollection services)
    {
        services.AddCortexMediator([typeof(PingRequestHandler)], options =>
        {
            options.AddOpenQueryPipelineBehavior(typeof(LoggingPipeline<,>));
            options.AddOpenQueryPipelineBehavior(typeof(PreProcessPipeline<,>));
            options.AddOpenQueryPipelineBehavior(typeof(PostProcessPipeline<,>));
        });

        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(FeatureControlPipeline<,>));
        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(CheckUserRolePipeline<,>));
        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(CheckRestrictionPipeline<,>));
        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(CheckMessagePipeline<,>));
        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(CheckAntispamPipeline<,>));
        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(CheckUsernamePipeline<,>));
        services.AddTransient(typeof(ITelegramPreProcessPipeline<,>), typeof(ActionResultPipeline<,>));
        services.AddTransient(typeof(ITelegramPostProcessPipeline<,>), typeof(CheckAfkSessionPipeline<,>));
        services.AddTransient(typeof(ITelegramPostProcessPipeline<,>), typeof(EnsureChatAdminPipeline<,>));
        services.AddTransient(typeof(ITelegramPostProcessPipeline<,>), typeof(EnsureChatSettingPipeline<,>));
        services.AddTransient(typeof(ITelegramPostProcessPipeline<,>), typeof(UpsertBotUserPipeline<,>));
        services.AddTransient(typeof(ITelegramPostProcessPipeline<,>), typeof(InsertChatActivityPipeline<,>));
        services.AddScoped<IAppMediator, CortexAppMediator>();

        return services;
    }
}
