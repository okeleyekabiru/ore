namespace Ore.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
