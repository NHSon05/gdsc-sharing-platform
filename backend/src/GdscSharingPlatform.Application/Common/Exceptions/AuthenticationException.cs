namespace GdscSharingPlatform.Application.Common.Exceptions;

public sealed class AuthenticationException(string message)
        : Exception(message)
{
}