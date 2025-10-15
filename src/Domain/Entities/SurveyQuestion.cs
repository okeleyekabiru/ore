using System.Collections.Generic;
using System.Linq;
using Ore.Domain.Common;
using Ore.Domain.Enums;

namespace Ore.Domain.Entities;

public sealed class SurveyQuestion : BaseEntity
{
    private readonly List<SurveyOption> _options = new();

    private SurveyQuestion()
    {
    }

    public SurveyQuestion(Guid surveyId, string prompt, SurveyQuestionType type, int order, IEnumerable<string> options)
    {
        SurveyId = surveyId;
        Prompt = prompt.Trim();
        Type = type;
        Order = order;
        SetOptions(options);
    }

    public Guid SurveyId { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public SurveyQuestionType Type { get; private set; }
    public int Order { get; private set; }

    public IReadOnlyCollection<SurveyOption> Options => _options;

    private void SetOptions(IEnumerable<string> options)
    {
        _options.Clear();
        foreach (var option in options.Select(o => o.Trim()).Where(o => !string.IsNullOrWhiteSpace(o)))
        {
            _options.Add(new SurveyOption(Id, option));
        }
    }

    public void Update(string prompt, SurveyQuestionType type, int order, IEnumerable<string> options)
    {
        Prompt = prompt.Trim();
        Type = type;
        Order = order;
        SetOptions(options);
    }
}
