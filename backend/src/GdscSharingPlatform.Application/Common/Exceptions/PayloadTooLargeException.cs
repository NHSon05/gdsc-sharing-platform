namespace GdscSharingPlatform.Application.Common.Exceptions;

public sealed class PayloadTooLargeException : Exception
{
    public PayloadTooLargeException(string message = "File size exceeds allowed limit.")
        : base(message)
    {
    }
}
