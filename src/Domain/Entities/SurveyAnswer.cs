using Ore.Domain.Common;

namespace Ore.Domain.Entities;

public sealed class SurveyAnswer : BaseEntity
{
    private SurveyAnswer()
    {
    }

    public SurveyAnswer(Guid submissionId, Guid questionId, string value, string? metadata)
    {
        SubmissionId = submissionId;
        QuestionId = questionId;
        Value = value;
        Metadata = metadata;
    }

    public Guid SubmissionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? Metadata { get; private set; }
}
