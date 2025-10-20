using FluentValidation;

namespace Ore.Application.Features.Content.Commands;

public sealed class GenerateContentCommandValidator : AbstractValidator<GenerateContentCommand>
{
    public GenerateContentCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.RequestedBy).NotEmpty();
        RuleFor(x => x.Topic).NotEmpty();
        RuleFor(x => x.Tone).NotEmpty();
        RuleFor(x => x.Platform).IsInEnum();
    }
}
