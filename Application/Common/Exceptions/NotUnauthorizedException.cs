using System.Runtime.Serialization;

namespace Application.Common.Exceptions;

[Serializable]
public class NotUnauthorizedException : Exception
{
    public NotUnauthorizedException()
    {
    }

    public NotUnauthorizedException(string message)
        : base(message)
    {
    }

    public NotUnauthorizedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected NotUnauthorizedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}