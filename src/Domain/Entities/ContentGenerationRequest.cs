using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class ContentGenerationRequest : AuditableEntity, IAggregateRoot
{
    private ContentGenerationRequest()
    {
    }

    public ContentGenerationRequest(Guid teamId, Guid requestedBy, string prompt, string model)
    {
        TeamId = teamId;
        RequestedBy = requestedBy;
        Prompt = prompt;
        Model = model;
        Status = ContentStatus.Generated;
    }

    public Guid TeamId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public ContentStatus Status { get; private set; }
    public Guid? ContentItemId { get; private set; }
    public ContentItem? ContentItem { get; private set; }
    public string? RawResponse { get; private set; }

    public void AttachContent(ContentItem item, string rawResponse)
    {
        ContentItem = item;
        ContentItemId = item.Id;
        RawResponse = rawResponse;
    }
}
