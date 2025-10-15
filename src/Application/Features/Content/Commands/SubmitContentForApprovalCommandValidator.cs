using FluentValidation;

namespace Ore.Application.Features.Content.Commands;

public sealed class SubmitContentForApprovalCommandValidator : AbstractValidator<SubmitContentForApprovalCommand>
{
    public SubmitContentForApprovalCommandValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.RequestedBy).NotEmpty();
    }
}
