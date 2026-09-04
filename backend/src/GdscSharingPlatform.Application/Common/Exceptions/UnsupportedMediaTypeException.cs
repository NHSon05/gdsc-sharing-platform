namespace GdscSharingPlatform.Application.Common.Exceptions;

public sealed class UnsupportedMediaTypeException : Exception
{
    public UnsupportedMediaTypeException(string message = "Unsupported media type or invalid file signature.")
        : base(message)
    {
    }
}
