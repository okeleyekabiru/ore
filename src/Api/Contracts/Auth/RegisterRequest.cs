using System.Collections.Generic;
using Ore.Domain.Enums;

namespace Ore.Api.Contracts.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    RoleType Role,
    string? TeamName,
    bool IsIndividual,
    BrandSurveyOnboardingRequest BrandSurvey);

public sealed record BrandSurveyOnboardingRequest(
    string Voice,
    string Tone,
    string Goals,
    string Audience,
    string Competitors,
    IEnumerable<string> Keywords);
