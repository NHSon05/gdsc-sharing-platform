using FluentValidation.TestHelper;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Application.Features.Auth.Validators;

namespace GdscSharingPlatform.UnitTests.Features.Auth.Validators;

public class AuthValidatorsTests
{
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RefreshTokenRequestValidator _refreshValidator = new();
    private readonly LogoutRequestValidator _logoutValidator = new();

    [Fact]
    public void LoginValidator_WithValidData_ShouldNotHaveValidationError()
    {
        var request = new LoginRequest("admin@gdsc.app", "Password123!");
        var result = _loginValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public void LoginValidator_WithInvalidEmail_ShouldHaveValidationError(string email)
    {
        var request = new LoginRequest(email, "Password123!");
        var result = _loginValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LoginValidator_WithEmptyPassword_ShouldHaveValidationError(string password)
    {
        var request = new LoginRequest("admin@gdsc.app", password);
        var result = _loginValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RefreshTokenValidator_WithValidToken_ShouldNotHaveValidationError()
    {
        var request = new RefreshTokenRequest("valid_refresh_token_string");
        var result = _refreshValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RefreshTokenValidator_WithEmptyToken_ShouldHaveValidationError(string token)
    {
        var request = new RefreshTokenRequest(token);
        var result = _refreshValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void LogoutValidator_WithValidToken_ShouldNotHaveValidationError()
    {
        var request = new LogoutRequest("valid_refresh_token_string");
        var result = _logoutValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LogoutValidator_WithEmptyToken_ShouldHaveValidationError(string token)
    {
        var request = new LogoutRequest(token);
        var result = _logoutValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
