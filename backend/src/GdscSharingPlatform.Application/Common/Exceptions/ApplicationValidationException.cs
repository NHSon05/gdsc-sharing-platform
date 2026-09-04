// 400 BadRequest

namespace GdscSharingPlatform.Application.Common.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException(
        IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ApplicationValidationException(string propertyName, string errorMessage)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]> { [propertyName] = [errorMessage] };
    }

    public ApplicationValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]> { ["general"] = [message] };
    }

    public IReadOnlyDictionary<string, string[]> Errors
    {
        get;
    }
}