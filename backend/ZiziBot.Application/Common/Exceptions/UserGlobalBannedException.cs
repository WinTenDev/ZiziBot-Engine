using System.Runtime.Serialization;

namespace ZiziBot.Application.Common.Exceptions;

[Serializable()]
public class UserGlobalBannedException : Exception
{
    protected UserGlobalBannedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public UserGlobalBannedException(long userId) : base($"UserId: {userId} is banned from some FedBan federation")
    {
    }
}