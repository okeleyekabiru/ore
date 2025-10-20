using Ore.Api.Contracts.Auth;
using Ore.Api.Validators.Auth;
using Ore.Domain.Enums;

namespace Ore.Api.Tests.Validators.Auth;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static BrandSurveyOnboardingRequest CreateBrandSurveyPayload() => new(
        "Confident",
        "Friendly",
        "Marketing leaders",
        "Grow awareness",
        "Legacy brands",
        new[] { "Helpful", "Approachable" });

    [Fact]
    public void Validate_WithValidInput_ReturnsValidResult()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "Password123!",
            "Jane",
            "Doe",
            RoleType.Admin,
            null,
            true,
            CreateBrandSurveyPayload());

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
        var request = new RegisterRequest(
            email ?? string.Empty,
            "Password123!",
            "Jane",
            "Doe",
            RoleType.Admin,
            null,
            true,
            CreateBrandSurveyPayload());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_InvalidPassword_ReturnsError(string? password)
    {
        var request = new RegisterRequest(
            "user@example.com",
            password ?? string.Empty,
            "Jane",
            "Doe",
            RoleType.Admin,
            null,
            true,
            CreateBrandSurveyPayload());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingNames_ReturnsError(string? name)
    {
        var request = new RegisterRequest(
            "user@example.com",
            "Password123!",
            name ?? string.Empty,
            name ?? string.Empty,
            RoleType.Admin,
            null,
            true,
            CreateBrandSurveyPayload());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.FirstName) || e.PropertyName == nameof(RegisterRequest.LastName));
    }

    [Fact]
    public void Validate_TeamRegistrationWithoutTeamName_ReturnsError()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "Password123!",
            "Jane",
            "Doe",
            RoleType.SocialMediaManager,
            null,
            false,
            CreateBrandSurveyPayload());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.TeamName));
    }
}
