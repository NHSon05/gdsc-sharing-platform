namespace GdscSharingPlatform.Api.Realtime;

public sealed class RealtimeOptions
{
    public int SubscribePerMinute { get; set; } = 60;
    public int MaxSubscriptions { get; set; } = 20;
    public int SessionRecheckSeconds { get; set; } = 30;
}
