using JetBrains.Annotations;

namespace ZiziBot.Application.Core.Interfaces;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IStartupTask
{
    Task ExecuteAsync();
}