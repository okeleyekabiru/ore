using System;
using System.Collections.Generic;
using Ore.Domain.Enums;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record BrandSurveyDetailsResponse(
    Guid Id,
    Guid TeamId,
    string Title,
    string Description,
    bool IsActive,
    IEnumerable<BrandSurveyQuestionDetails> Questions);

public sealed record BrandSurveyQuestionDetails(
    Guid Id,
    string Prompt,
    SurveyQuestionType Type,
    int Order,
    IEnumerable<string> Options);
