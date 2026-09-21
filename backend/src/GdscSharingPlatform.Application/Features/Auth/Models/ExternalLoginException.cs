namespace GdscSharingPlatform.Application.Features.Auth.Models;

public sealed class ExternalLoginException : Exception
{
    public string Code {get;}
    public ExternalLoginException(string code) : base("External login could not be completed")
    {
        Code = code;
    }
}