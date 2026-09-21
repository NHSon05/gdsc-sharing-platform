namespace GdscSharingPlatform.Infrastructure.Identity.Options;

public sealed class GoogleAuthenticationOptions
{
    public const string SectionName = "Authentication:Google";
    public string ClientId {get; set;} = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;
}