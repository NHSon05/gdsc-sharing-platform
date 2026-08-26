using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GdscSharingPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor
    )
    {
        _httpContextAccessor = httpContextAccessor;
    }
    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
    public Guid? UserId
    {
        get
        {
            var principal = Principal;

            if (principal is null)
            {
                return null;
            }

            var subject =
                principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(subject, out var userId) ? userId : null;
        }

    }
    public string? Email
    {
        get
        {
            var principal = Principal;

            return principal?
                .FindFirst(JwtRegisteredClaimNames.Email)?
                .Value
                ?? principal?
                    .FindFirst(ClaimTypes.Email)?
                    .Value;
        }
    }
    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var principal = Principal;

            if (principal is null)
            {
                return Array.Empty<string>();
            }

            return principal
                .FindAll("role")
                .Select(claim => claim.Value)
                .Concat(
                    principal
                        .FindAll(ClaimTypes.Role)
                        .Select(claim => claim.Value))
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }
}
