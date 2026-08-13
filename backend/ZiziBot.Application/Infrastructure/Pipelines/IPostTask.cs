namespace ZiziBot.Application.Infrastructure.Pipelines;

public interface IPostTask
{
    void Execute(PipelineBase pipelineBase);
}