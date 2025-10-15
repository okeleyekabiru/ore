using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Events;

public sealed class ContentApprovalEvent : IDomainEvent
{
    public ContentApprovalEvent(Guid contentId, Guid approverId, ApprovalStatus status)
    {
        ContentId = contentId;
        ApproverId = approverId;
        Status = status;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid ContentId { get; }
    public Guid ApproverId { get; }
    public ApprovalStatus Status { get; }
    public DateTime OccurredOnUtc { get; }
}
