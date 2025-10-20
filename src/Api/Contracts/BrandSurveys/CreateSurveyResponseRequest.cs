using System;
using System.Collections.Generic;

namespace Ore.Api.Contracts.BrandSurveys;

public sealed record CreateSurveyResponseRequest(
    Guid SurveyId,
    Guid? UserId,
    IEnumerable<SurveyAnswerRequest> Answers,
    BrandVoiceProfileRequest? VoiceProfile);
