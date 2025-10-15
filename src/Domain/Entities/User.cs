using System.Linq;
using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class User : AuditableEntity, IAggregateRoot
{
    private readonly List<ContentItem> _authoredContent = new();
    private readonly List<ApprovalRecord> _approvals = new();

    private User()
    {
    }

    public User(Guid id, string email, string firstName, string lastName, RoleType role)
    {
        Id = id;
        Email = email.Trim().ToLowerInvariant();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Role = role;
    }

    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public RoleType Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Guid? TeamId { get; private set; }
    public Team? Team { get; private set; }

    public IReadOnlyCollection<ContentItem> AuthoredContent => _authoredContent;
    public IReadOnlyCollection<ApprovalRecord> Approvals => _approvals;

    public string FullName => string.Join(' ', new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public void AssignTeam(Team team)
    {
        Team = team;
        TeamId = team?.Id;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void ChangeRole(RoleType role) => Role = role;

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
