using FluentValidation;

namespace Ore.Application.Features.Content.Commands;

public sealed class RejectContentCommandValidator : AbstractValidator<RejectContentCommand>
{
    public RejectContentCommandValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}
