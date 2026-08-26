namespace GdscSharingPlatform.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        IEnumerable<string> roles,
        Guid? departmentId,
        string status
    );
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}