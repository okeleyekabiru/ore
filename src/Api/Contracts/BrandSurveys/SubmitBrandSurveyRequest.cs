using System;
using System.Collections.Generic;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record SubmitBrandSurveyRequest(
    Guid? UserId,
    IEnumerable<SurveyAnswerRequest> Answers,
    BrandVoiceProfileRequest? VoiceProfile);

public sealed record SurveyAnswerRequest(
    Guid QuestionId,
    string Value,
    string? Metadata);

public sealed record BrandVoiceProfileRequest(
    string Voice,
    string Tone,
    string Audience,
    IEnumerable<string> Keywords);
