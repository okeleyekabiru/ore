using System.Collections.Generic;
using Ore.Domain.Enums;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record UpdateBrandSurveyRequest(
    string Title,
    string Description,
    IEnumerable<UpdateSurveyQuestionRequest> Questions);

public sealed record UpdateSurveyQuestionRequest(
    string Prompt,
    SurveyQuestionType Type,
    int Order,
    IEnumerable<string> Options);
