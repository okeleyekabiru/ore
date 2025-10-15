using FluentValidation;

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed class CreateBrandSurveyCommandValidator : AbstractValidator<CreateBrandSurveyCommand>
{
    public CreateBrandSurveyCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Questions)
            .NotEmpty();
    }
}
