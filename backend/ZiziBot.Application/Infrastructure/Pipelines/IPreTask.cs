namespace ZiziBot.Application.Infrastructure.Pipelines;

public interface IPreTask
{
    void Execute(PipelineBase pipelineBase, NextAction next);
}