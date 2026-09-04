using FluentValidation.TestHelper;
using GdscSharingPlatform.Application.Features.Profile.Models;
using GdscSharingPlatform.Application.Features.Profile.Validators;

namespace GdscSharingPlatform.UnitTests.Features.Profile.Validators;

public class ProfileValidatorsTests
{
    private readonly UpdateProfileRequestValidator _updateProfileValidator = new();
    private readonly ChangeEmailRequestValidator _changeEmailValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a")]
    public void UpdateProfile_InvalidDisplayName_ShouldHaveValidationError(string displayName)
    {
        var request = new UpdateProfileRequest(displayName, null, null, null, null, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void UpdateProfile_ValidDisplayName_ShouldPass()
    {
        var request = new UpdateProfileRequest("Nguyen Van A", null, null, null, null, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DisplayName);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@test.com")]
    public void UpdateProfile_InvalidEmail_ShouldHaveValidationError(string email)
    {
        var request = new UpdateProfileRequest("Valid Name", email, null, null, null, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void UpdateProfile_ValidEmail_ShouldPass()
    {
        var request = new UpdateProfileRequest("Valid Name", "new@example.com", null, null, null, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-phone")]
    [InlineData("123")]
    public void UpdateProfile_InvalidPhone_ShouldHaveValidationError(string phone)
    {
        var request = new UpdateProfileRequest("Valid Name", null, phone, null, null, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("+84901234567")]
    [InlineData("0901234567")]
    public void UpdateProfile_ValidPhone_ShouldPass(string phone)
    {
        var request = new UpdateProfileRequest("Valid Name", null, phone, null, null, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("http://github.com/user")]
    [InlineData("https://gitlab.com/user")]
    [InlineData("not-a-url")]
    public void UpdateProfile_InvalidGithubUrl_ShouldHaveValidationError(string githubUrl)
    {
        var request = new UpdateProfileRequest("Valid Name", null, null, null, githubUrl, null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.GithubUrl);
    }

    [Fact]
    public void UpdateProfile_ValidGithubUrl_ShouldPass()
    {
        var request = new UpdateProfileRequest("Valid Name", null, null, null, "https://github.com/nguyenvanan", null);
        var result = _updateProfileValidator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.GithubUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void ChangeEmail_InvalidEmail_ShouldHaveValidationError(string email)
    {
        var request = new ChangeEmailRequest(email);
        var result = _changeEmailValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail);
    }

    [Fact]
    public void ChangeEmail_ValidEmail_ShouldPass()
    {
        var request = new ChangeEmailRequest("new@example.com");
        var result = _changeEmailValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
