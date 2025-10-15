using System;
using FluentValidation;

namespace Ore.Application.Features.Scheduling.Commands;

public sealed class ScheduleContentCommandValidator : AbstractValidator<ScheduleContentCommand>
{
    public ScheduleContentCommandValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.PublishOnUtc)
            .Must(date => date > DateTime.UtcNow)
            .WithMessage("PublishOnUtc must be in the future");
        RuleFor(x => x.MaxRetryCount).GreaterThanOrEqualTo(0);
    }
}
