namespace GdscSharingPlatform.Application.Common.Exceptions;

public sealed class PreconditionFailedException() : Exception("The version is stale. Reload the resource before updating it.");
