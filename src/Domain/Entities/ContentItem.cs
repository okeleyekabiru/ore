using System.Collections.Generic;
using System.Linq;
using Ore.Domain.Common;
using Ore.Domain.Enums;
using Ore.Domain.ValueObjects;

namespace Ore.Domain.Entities;

public sealed class ContentItem : AuditableEntity, IAggregateRoot
{
    private readonly List<string> _hashtags = new();
    private readonly List<ContentDistribution> _distributions = new();
    private readonly List<ApprovalRecord> _approvalHistory = new();

    private ContentItem()
    {
    }

    public ContentItem(Guid teamId, Guid? authorId, string title, string body, string? caption)
    {
        TeamId = teamId;
        AuthorId = authorId;
        Title = title.Trim();
        Body = body.Trim();
        Caption = caption;
        Status = ContentStatus.Draft;
    }

    public Guid TeamId { get; private set; }
    public Guid? AuthorId { get; private set; }
    public User? Author { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Caption { get; private set; }
    public ContentStatus Status { get; private set; }
    public Guid? CurrentApprovalId { get; private set; }

    public IReadOnlyCollection<string> Hashtags => _hashtags;
    public IReadOnlyCollection<ContentDistribution> Distributions => _distributions;
    public IReadOnlyCollection<ApprovalRecord> ApprovalHistory => _approvalHistory;

    public void UpdateContent(string title, string body, string? caption, IEnumerable<string> hashtags)
    {
        Title = title.Trim();
        Body = body.Trim();
        Caption = caption;

        _hashtags.Clear();
        _hashtags.AddRange(hashtags.Select(h => h.Trim()).Where(h => !string.IsNullOrWhiteSpace(h)));
    }

    public void ApplyBrandVoice(BrandVoiceProfile? profile)
    {
        if (profile is null)
        {
            return;
        }

        if (!Hashtags.Any() && profile.Keywords.Any())
        {
            _hashtags.AddRange(profile.Keywords.Select(k => $"#{k.Replace(' ', '_').ToLowerInvariant()}"));
        }
    }

    public void ResetToDraft()
    {
        Status = ContentStatus.Draft;
        CurrentApprovalId = null;
    }

    public void MarkGenerated()
    {
        Status = ContentStatus.Generated;
        CurrentApprovalId = null;
    }
    public void SubmitForApproval()
    {
        CurrentApprovalId = null;
        Status = ContentStatus.PendingApproval;
    }
    public void Approve(Guid approvalId)
    {
        CurrentApprovalId = approvalId;
        Status = ContentStatus.Approved;
    }

    public void Reject(Guid approvalId)
    {
        CurrentApprovalId = approvalId;
        Status = ContentStatus.Rejected;
    }

    public void Schedule(ContentDistribution distribution)
    {
        _distributions.Add(distribution);
        Status = ContentStatus.Scheduled;
    }

    public void MarkPublished()
    {
        Status = ContentStatus.Published;
    }

    public void AddApprovalRecord(ApprovalRecord record)
    {
        _approvalHistory.Add(record);
    }
}
