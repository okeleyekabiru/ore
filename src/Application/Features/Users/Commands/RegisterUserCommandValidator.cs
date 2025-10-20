using FluentValidation;

namespace Ore.Application.Features.Users.Commands;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();

        RuleFor(x => x.Role)
            .IsInEnum();

        RuleFor(x => x.TeamName)
            .NotEmpty()
            .When(x => !x.IsIndividual)
            .WithMessage("Team name is required when registering as a team.");

        RuleFor(x => x.BrandSurvey)
            .NotNull();

        When(x => x.BrandSurvey is not null, () =>
        {
            RuleFor(x => x.BrandSurvey!.Voice)
                .NotEmpty();

            RuleFor(x => x.BrandSurvey!.Tone)
                .NotEmpty();
        });
    }
}
