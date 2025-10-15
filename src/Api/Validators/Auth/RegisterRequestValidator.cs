using FluentValidation;
using Ore.Api.Contracts.Auth;

namespace Ore.Api.Validators.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
	public RegisterRequestValidator()
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
	}
}
