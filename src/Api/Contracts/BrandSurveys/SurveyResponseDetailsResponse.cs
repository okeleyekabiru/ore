using System;
using System.Collections.Generic;
using Ore.Domain.Enums;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record SurveyResponseDetailsResponse(
    Guid SubmissionId,
    Guid SurveyId,
    Guid UserId,
    string SurveyTitle,
    string SurveyCategory,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc,
    IEnumerable<SurveyResponseAnswerDetails> Answers);

public sealed record SurveyResponseAnswerDetails(
    Guid QuestionId,
    string Prompt,
    SurveyQuestionType Type,
    string Value,
    string? Metadata);
