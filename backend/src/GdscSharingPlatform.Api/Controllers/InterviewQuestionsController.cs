using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Interviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/interview-questions")]
[Route("api/v1/interview-questions")]
[Route("api/interview-question")]
[Route("api/v1/interview-question")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Member)]
public sealed class InterviewQuestionsController(IInterviewQuestionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<InterviewQuestionPage>> List([FromQuery] InterviewQuestionQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, false, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InterviewQuestionDetail>> Get(Guid id, CancellationToken ct)
        => Ok(await service.GetAsync(id, false, ct));
}

[ApiController]
[Route("api/admin/interview-questions")]
[Route("api/v1/admin/interview-questions")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminInterviewQuestionsController(IInterviewQuestionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<InterviewQuestionPage>> List([FromQuery] InterviewQuestionQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, true, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InterviewQuestionDetail>> Get(Guid id, CancellationToken ct)
        => Ok(await service.GetAsync(id, true, ct));
}
