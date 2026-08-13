using System.Runtime.Serialization;

namespace ZiziBot.Application.Common.Exceptions;

[Serializable()]
public class EnvMissingException : Exception
{
    protected EnvMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public EnvMissingException(string envName) : base($"Env: '{envName}' is missing in environment variable. Please ensure '{envName}' in your environment or .env")
    {
    }
}