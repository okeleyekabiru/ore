using System;
using System.Collections.Generic;
using System.Linq;
using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class BrandSurvey : AuditableEntity, IAggregateRoot
{
    private readonly List<SurveyQuestion> _questions = new();

    private BrandSurvey()
    {
    }

    public BrandSurvey(Guid teamId, string title, string description)
    {
        TeamId = teamId;
        Title = title.Trim();
        Description = description.Trim();
    }

    public Guid TeamId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<SurveyQuestion> Questions => _questions;

    public void UpdateDetails(string title, string description)
    {
        Title = title.Trim();
        Description = description.Trim();
    }

    public void AddQuestion(string prompt, SurveyQuestionType type, int order, IEnumerable<string>? options = null)
    {
        var question = new SurveyQuestion(Id, prompt, type, order, options ?? Array.Empty<string>());
        _questions.Add(question);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void RemoveQuestions(IEnumerable<Guid> questionIds)
    {
        var ids = questionIds is HashSet<Guid> set
            ? set
            : new HashSet<Guid>(questionIds);

        _questions.RemoveAll(question => ids.Contains(question.Id));
    }
}
