using GdscSharingPlatform.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/authorization-test")]
public sealed class AuthorizationTestController : ControllerBase
{
    [Authorize(Policy = AuthPolicies.RequireActiveUser)]
    [HttpGet("authenticated")]
    public IActionResult Authenticated()
    {
        return Ok(new
        {
            Message = "You are authenticated and active."
        });
    }

    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            Message = "Admin policy passed."
        });
    }

    [Authorize(Policy = AuthPolicies.MemberOnly)]
    [HttpGet("member")]
    public IActionResult Member()
    {
        return Ok(new
        {
            Message = "Member policy passed."
        });
    }
}