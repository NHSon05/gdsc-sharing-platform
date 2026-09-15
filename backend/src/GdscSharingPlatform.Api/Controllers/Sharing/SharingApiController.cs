using System.Globalization;
using GdscSharingPlatform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 409)]
[ProducesResponseType(typeof(ProblemDetails), 412)]
[ProducesResponseType(typeof(ProblemDetails), 429)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public abstract class SharingApiController : ControllerBase
{
    protected long Version()
    {
        var value = Request.Headers.IfMatch.ToString();
        if (value.Length < 3 || value[0] != '"' || value[^1] != '"'
            || !long.TryParse(value[1..^1], NumberStyles.None, CultureInfo.InvariantCulture, out var version))
            throw new ApplicationValidationException("If-Match", "Supply one quoted version, for example: \"12\".");
        return version;
    }
    protected void ETag(long version) => Response.Headers.ETag = $"\"{version}\"";
}
