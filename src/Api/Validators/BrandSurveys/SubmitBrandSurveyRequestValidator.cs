using FluentValidation;
using Ore.Api.Contracts.BrandSurveys;

namespace Ore.Api.Validators.BrandSurveys;

public sealed class SubmitBrandSurveyRequestValidator : AbstractValidator<SubmitBrandSurveyRequest>
{
    public SubmitBrandSurveyRequestValidator()
    {
        RuleFor(x => x.Answers).NotEmpty();
        RuleForEach(x => x.Answers).SetValidator(new SurveyAnswerRequestValidator());

        When(x => x.VoiceProfile is not null, () =>
        {
            RuleFor(x => x.VoiceProfile!).SetValidator(new BrandVoiceProfileRequestValidator());
        });
    }

    private sealed class SurveyAnswerRequestValidator : AbstractValidator<SurveyAnswerRequest>
    {
        public SurveyAnswerRequestValidator()
        {
            RuleFor(x => x.QuestionId).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
            RuleFor(x => x.Metadata).MaximumLength(2000).When(x => x.Metadata is not null);
        }
    }

    private sealed class BrandVoiceProfileRequestValidator : AbstractValidator<BrandVoiceProfileRequest>
    {
        public BrandVoiceProfileRequestValidator()
        {
            RuleFor(x => x.Voice).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Tone).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Audience).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Keywords).NotNull();
        }
    }
}
