using FluentValidation;

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed class UpdateBrandSurveyCommandValidator : AbstractValidator<UpdateBrandSurveyCommand>
{
    public UpdateBrandSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Questions).NotEmpty();
    }
}
