using System;
using FluentValidation;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Content.Commands;

public sealed class CreateContentItemCommandValidator : AbstractValidator<CreateContentItemCommand>
{
    public CreateContentItemCommandValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);

        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => status is ContentStatus.Draft or ContentStatus.Generated or ContentStatus.PendingApproval or ContentStatus.Scheduled)
            .WithMessage("Status must be Draft, Generated, PendingApproval, or Scheduled for new content items.");

        When(x => x.Status == ContentStatus.Scheduled, () =>
        {
            RuleFor(x => x.DueOnUtc)
                .NotNull()
                .Must(value => value.HasValue && value.Value > DateTime.UtcNow.AddMinutes(-1))
                .WithMessage("Scheduled items require a future publish window.");

            RuleFor(x => x.Platform)
                .NotNull()
                .WithMessage("Scheduled items require a channel.");
        });
    }
}