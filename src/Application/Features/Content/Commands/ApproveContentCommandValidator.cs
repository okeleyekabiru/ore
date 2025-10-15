using FluentValidation;

namespace Ore.Application.Features.Content.Commands;

public sealed class ApproveContentCommandValidator : AbstractValidator<ApproveContentCommand>
{
    public ApproveContentCommandValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
    }
}
