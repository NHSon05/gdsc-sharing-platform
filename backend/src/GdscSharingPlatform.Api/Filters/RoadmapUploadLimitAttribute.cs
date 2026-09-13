using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GdscSharingPlatform.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RoadmapUploadLimitAttribute : Attribute, IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.HttpContext.Request.ContentLength > RoadmapStorageOptions.MaximumRequestBytes)
            throw new PayloadTooLargeException();
        await next();
    }
}
