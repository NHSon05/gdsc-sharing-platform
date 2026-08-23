namespace GdscSharingPlatform.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(
        string resourceName,
        object key)
        : base(
            $"{resourceName} with identifier '{key}' was not found.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    public string ResourceName { get; }

    public object Key { get; }
}