namespace GdscSharingPlatform.Application.Features.Memberships.Models;

public sealed record GenerationDto(
    Guid Id,
    int Number,
    string Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive);

public sealed record CreateGenerationRequest(
    int Number,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record UpdateGenerationRequest(
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool? IsActive);
