namespace GdscSharingPlatform.Application.Features.Sharing;

public interface IRealtimeEventPublisher
{
    Task PublishAsync(Guid eventId, string eventName, string payloadJson, CancellationToken ct);
}
