using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class ApprovalRecord : AuditableEntity, IAggregateRoot
{
    private ApprovalRecord()
    {
    }

    public ApprovalRecord(Guid contentItemId, Guid approverId, ApprovalStatus status, string? comments)
    {
        ContentItemId = contentItemId;
        ApproverId = approverId;
        Status = status;
        Comments = comments;
    }

    public Guid ContentItemId { get; private set; }
    public Guid ApproverId { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public string? Comments { get; private set; }
}
