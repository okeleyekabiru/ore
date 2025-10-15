using Ore.Domain.Common;

namespace Ore.Domain.Entities;

public sealed class SurveyOption : BaseEntity
{
    private SurveyOption()
    {
    }

    public SurveyOption(Guid questionId, string value)
    {
        QuestionId = questionId;
        Value = value.Trim();
    }

    public Guid QuestionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
}
