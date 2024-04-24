using System.Runtime.Serialization;

namespace Application.Common.Exceptions;

[Serializable]
public class InvalidConfiguration : Exception
{
    public InvalidConfiguration()
    {
    }

    public InvalidConfiguration(string message)
        : base(message)
    {
    }

    public InvalidConfiguration(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected InvalidConfiguration(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}