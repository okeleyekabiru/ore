using System;
using System.Collections.Generic;
using Ore.Domain.Enums;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record CreateBrandSurveyRequest(
    Guid TeamId,
    string Title,
    string Description,
    IEnumerable<CreateSurveyQuestionRequest> Questions);

public sealed record CreateSurveyQuestionRequest(
    string Prompt,
    SurveyQuestionType Type,
    int Order,
    IEnumerable<string> Options);
