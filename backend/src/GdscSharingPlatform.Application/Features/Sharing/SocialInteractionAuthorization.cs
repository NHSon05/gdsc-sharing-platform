using GdscSharingPlatform.Application.Common.Exceptions;

namespace GdscSharingPlatform.Application.Features.Sharing;

public static class SocialInteractionAuthorization
{
    public static void RequireCommentAuthor(Guid authorUserId, Guid currentUserId, bool isAdmin)
    {
        if (authorUserId != currentUserId) throw new ForbiddenAccessException();
    }

    public static void RequireModeration(bool isAdmin)
    {
        if (!isAdmin) throw new ForbiddenAccessException();
    }

    public static void RequireScheduleAudience(bool isAudienceMember, bool isPresenter, bool isAdmin)
    {
        if (!isAudienceMember && !isPresenter && !isAdmin) throw new ForbiddenAccessException();
    }

    public static void RequireNotificationOwner(Guid recipientUserId, Guid currentUserId)
    {
        if (recipientUserId != currentUserId) throw new ForbiddenAccessException();
    }
}
