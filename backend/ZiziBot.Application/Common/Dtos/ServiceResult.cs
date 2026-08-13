namespace ZiziBot.Application.Common.Dtos;

public class ServiceResult
{
    public string Message { get; set; }

    public ServiceResult Complete(string message)
    {
        Message = message;
        return this;
    }

    public static ServiceResult Init()
    {
        return new();
    }
}