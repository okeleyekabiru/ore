using System;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record BrandSurveySummaryResponse(
    Guid Id,
    Guid TeamId,
    string Title,
    string Description,
    string Category,
    bool IsActive,
    int QuestionCount,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
