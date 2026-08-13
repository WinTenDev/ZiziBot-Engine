using System.Runtime.Serialization;

namespace ZiziBot.Application.Common.Exceptions;

[Serializable()]
public class SendMessageTextException : Exception
{
    protected SendMessageTextException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public SendMessageTextException(long chatId, string messageType) : base($"Fail when sending {messageType} to ChatId: {chatId}")
    {
    }
}