namespace GdscSharingPlatform.Application.Features.Memberships.Models;

public sealed record DepartmentDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder,
    bool IsActive);

public sealed record CreateDepartmentRequest(
    string Name,
    string Slug,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder);

public sealed record UpdateDepartmentRequest(
    string Name,
    string Slug,
    string? Description,
    string? Color,
    string? Icon,
    int SortOrder);
