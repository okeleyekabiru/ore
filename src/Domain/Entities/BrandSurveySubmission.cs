using System.Collections.Generic;
using System.Linq;
using Ore.Domain.Common;

namespace Ore.Domain.Entities;

public sealed class BrandSurveySubmission : AuditableEntity, IAggregateRoot
{
    private readonly List<SurveyAnswer> _answers = new();

    private BrandSurveySubmission()
    {
    }

    public BrandSurveySubmission(Guid surveyId, Guid userId)
    {
        SurveyId = surveyId;
        UserId = userId;
    }

    public Guid SurveyId { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<SurveyAnswer> Answers => _answers;

    public void AddAnswer(Guid questionId, string value, string? metadata = null)
    {
        var answer = new SurveyAnswer(Id, questionId, value, metadata);
        _answers.Add(answer);
    }

    public void ReplaceAnswers(IEnumerable<SurveyAnswer> answers)
    {
        _answers.Clear();
        _answers.AddRange(answers);
    }
}
