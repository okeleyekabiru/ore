using System.Linq;
using FluentValidation;
using Ore.Api.Contracts.BrandSurveys;
using Ore.Domain.Enums;

namespace Ore.Api.Validators.BrandSurveys;

public sealed class UpdateBrandSurveyRequestValidator : AbstractValidator<UpdateBrandSurveyRequest>
{
    public UpdateBrandSurveyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Questions).NotEmpty();
        RuleForEach(x => x.Questions).SetValidator(new UpdateSurveyQuestionRequestValidator());
    }

    private sealed class UpdateSurveyQuestionRequestValidator : AbstractValidator<UpdateSurveyQuestionRequest>
    {
        public UpdateSurveyQuestionRequestValidator()
        {
            RuleFor(x => x.Prompt).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Order).GreaterThanOrEqualTo(0);

            RuleFor(x => x.Options)
                .Cascade(CascadeMode.Stop)
                .Must(options => options is not null)
                .When(x => IsChoiceQuestion(x.Type))
                .WithMessage("Options must be provided for choice questions.")
                .Must(options => options is not null && options.Any(o => !string.IsNullOrWhiteSpace(o)))
                .When(x => IsChoiceQuestion(x.Type))
                .WithMessage("Choice questions must include at least one option.");
        }

        private static bool IsChoiceQuestion(SurveyQuestionType type) =>
            type is SurveyQuestionType.SingleChoice or SurveyQuestionType.MultiChoice;
    }
}
