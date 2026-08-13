using System.Runtime.Serialization;

namespace ZiziBot.Application.Common.Exceptions;

[Serializable()]
public class BotMiddlewareException<T> : Exception
{
    public T? Result { get; set; }

    protected BotMiddlewareException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public BotMiddlewareException(string message, T? result = default) : base(message)
    {
        Result = result;
    }
}