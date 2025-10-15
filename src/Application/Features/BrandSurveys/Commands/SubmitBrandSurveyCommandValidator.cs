using FluentValidation;

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed class SubmitBrandSurveyCommandValidator : AbstractValidator<SubmitBrandSurveyCommand>
{
    public SubmitBrandSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty();
    }
}
