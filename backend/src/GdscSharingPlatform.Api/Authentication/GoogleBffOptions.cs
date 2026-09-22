namespace GdscSharingPlatform.Api.Authentication;

public sealed class GoogleBffOptions
{
    public string CallbackUrl { get; set; } = string.Empty;

    public static bool IsValid(GoogleBffOptions options, bool allowHttp)
    {
        return Uri.TryCreate(options.CallbackUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == "https" || (allowHttp && uri.Scheme == "http" && uri.IsLoopback))
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment)
            && uri.AbsolutePath == "/api/auth/google/callback";
    }
}
