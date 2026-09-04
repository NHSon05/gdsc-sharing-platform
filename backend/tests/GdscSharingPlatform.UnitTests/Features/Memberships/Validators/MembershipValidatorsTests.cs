using FluentValidation.TestHelper;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Application.Features.Memberships.Validators;

namespace GdscSharingPlatform.UnitTests.Features.Memberships.Validators;

public class MembershipValidatorsTests
{
    private readonly CreateGenerationRequestValidator _createGenValidator = new();
    private readonly CreateDepartmentRequestValidator _createDeptValidator = new();
    private readonly AssignMemberToGenRequestValidator _assignGenValidator = new();
    private readonly AddMemberToDepartmentRequestValidator _addDeptValidator = new();
    private readonly ReplaceRolesRequestValidator _replaceRolesValidator = new();

    [Fact]
    public void CreateGen_ZeroOrNegativeNumber_ShouldFail()
    {
        var request = new CreateGenerationRequest(0, null, null);
        var result = _createGenValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Number);
    }

    [Fact]
    public void CreateGen_StartDateAfterEndDate_ShouldFail()
    {
        var request = new CreateGenerationRequest(
            1,
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 1, 1));
        var result = _createGenValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Invalid Slug With Spaces")]
    [InlineData("slug_with_underscores")]
    public void CreateDept_InvalidSlug_ShouldFail(string slug)
    {
        var request = new CreateDepartmentRequest("Department Name", slug, null, null, null, 0);
        var result = _createDeptValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void CreateDept_ValidRequest_ShouldPass()
    {
        var request = new CreateDepartmentRequest("Software Engineering", "software-engineering", "Desc", "#3B82F6", "code", 1);
        var result = _createDeptValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddMemberToDepartment_EmptyRoles_ShouldFail()
    {
        var request = new AddMemberToDepartmentRequest(Guid.NewGuid(), true, new List<Guid>());
        var result = _addDeptValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RoleIds);
    }

    [Fact]
    public void ReplaceRoles_EmptyRoles_ShouldFail()
    {
        var request = new ReplaceRolesRequest(new List<Guid>());
        var result = _replaceRolesValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RoleIds);
    }
}
