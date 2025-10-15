using System.Linq;
using Ore.Domain.Common;
using Ore.Domain.ValueObjects;

namespace Ore.Domain.Entities;

public sealed class Team : AuditableEntity, IAggregateRoot
{
    private readonly List<User> _members = new();
    private readonly List<BrandSurvey> _surveys = new();
    private readonly List<BrandSurveySubmission> _submissions = new();
    private readonly List<ContentItem> _contentItems = new();

    private Team()
    {
    }

    public Team(string name)
    {
        Name = name.Trim();
    }

    public string Name { get; private set; } = string.Empty;
    public BrandVoiceProfile? BrandVoice { get; private set; }

    public IReadOnlyCollection<User> Members => _members;
    public IReadOnlyCollection<BrandSurvey> Surveys => _surveys;
    public IReadOnlyCollection<BrandSurveySubmission> Submissions => _submissions;
    public IReadOnlyCollection<ContentItem> ContentItems => _contentItems;

    public void UpdateName(string name) => Name = name.Trim();

    public void SetBrandVoice(BrandVoiceProfile profile)
    {
        BrandVoice = profile;
    }

    public void AddMember(User user)
    {
        if (_members.All(m => m.Id != user.Id))
        {
            _members.Add(user);
            user.AssignTeam(this);
        }
    }
}
