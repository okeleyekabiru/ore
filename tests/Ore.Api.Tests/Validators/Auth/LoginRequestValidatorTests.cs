using Ore.Api.Contracts.Auth;
using Ore.Api.Validators.Auth;

namespace Ore.Api.Tests.Validators.Auth;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ReturnsValidResult()
    {
        var request = new LoginRequest("user@example.com", "Password123!");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public void Validate_InvalidEmail_ReturnsError(string? email)
    {
        var request = new LoginRequest(email ?? string.Empty, "Password123!");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingPassword_ReturnsError(string? password)
    {
        var request = new LoginRequest("user@example.com", password ?? string.Empty);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Password));
    }
}
