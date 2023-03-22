using System.Runtime.Serialization;

namespace ZiziBot.Exceptions;

[Serializable()]
public class AppException : Exception
{
    protected AppException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public AppException(string message) : base(message)
    {
    }
}