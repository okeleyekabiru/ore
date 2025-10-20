import { useMemo, useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import PageHeader from '../../components/common/PageHeader';
import { useAuth } from '../../contexts/AuthContext';
import { createBrandSurvey } from '../../services/brandSurveyService';
import { ApiError } from '../../types/api';
import { SurveyQuestionType } from '../../types/brandSurvey';
import type { SurveyQuestionType as SurveyQuestionTypeValue } from '../../types/brandSurvey';
import './BrandSurveyCreatePage.css';

type QuestionDraft = {
  prompt: string;
  type: SurveyQuestionTypeValue;
  optionsText: string;
};

const DEFAULT_QUESTION: QuestionDraft = {
  prompt: '',
  type: SurveyQuestionType.Text,
  optionsText: '',
};

const OPTION_REQUIRED_TYPES: SurveyQuestionTypeValue[] = [
  SurveyQuestionType.SingleChoice,
  SurveyQuestionType.MultiChoice,
];

const questionTypeOptions = [
  { value: SurveyQuestionType.Text, label: 'Short answer' },
  { value: SurveyQuestionType.TextArea, label: 'Long form' },
  { value: SurveyQuestionType.SingleChoice, label: 'Single choice' },
  { value: SurveyQuestionType.MultiChoice, label: 'Multi choice' },
  { value: SurveyQuestionType.Scale, label: 'Scale (1-5)' },
];

const requiresOptions = (type: SurveyQuestionTypeValue) => OPTION_REQUIRED_TYPES.includes(type);

const normalizeOptions = (raw: string) =>
  raw
    .split(/\r?\n|,/)
    .map((item) => item.trim())
    .filter((item) => item.length > 0);

const BrandSurveyCreatePage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  const [teamId, setTeamId] = useState<string>(user?.teamId ?? '');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('Onboarding');
  const [questions, setQuestions] = useState<QuestionDraft[]>([{ ...DEFAULT_QUESTION }]);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const orderedQuestions = useMemo(
    () =>
      questions.map((question, index) => ({
        ...question,
        order: index + 1,
      })),
    [questions],
  );

  const handleQuestionChange = (index: number, field: keyof QuestionDraft, value: string) => {
    setQuestions((prev) => {
      const next = [...prev];
      const draft = { ...next[index] };

      if (field === 'type') {
        draft.type = Number(value) as SurveyQuestionTypeValue;
        if (!requiresOptions(draft.type)) {
          draft.optionsText = '';
        }
      } else if (field === 'prompt') {
        draft.prompt = value;
      } else {
        draft.optionsText = value;
      }

      next[index] = draft;
      return next;
    });
  };

  const handleAddQuestion = () => {
    setQuestions((prev) => [...prev, { ...DEFAULT_QUESTION }]);
  };

  const handleRemoveQuestion = (index: number) => {
    setQuestions((prev) => {
      if (prev.length === 1) {
        return prev;
      }
      const next = prev.filter((_, itemIndex) => itemIndex !== index);
      return next.length > 0 ? next : [DEFAULT_QUESTION];
    });
  };

  const validateForm = () => {
    if (!teamId) {
      return 'Provide a team identifier before creating the survey.';
    }

    if (!title.trim()) {
      return 'Survey title is required.';
    }

    if (!category.trim()) {
      return 'Provide a survey category to help organize templates.';
    }

    if (questions.length === 0) {
      return 'Add at least one question to the survey.';
    }

    for (const [index, question] of orderedQuestions.entries()) {
      if (!question.prompt.trim()) {
        return `Question ${index + 1} needs a prompt.`;
      }

      if (requiresOptions(question.type)) {
        const options = normalizeOptions(question.optionsText);
        if (options.length < 2) {
          return `Question ${index + 1} needs at least two options.`;
        }
      }
    }

    return null;
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setSuccess(null);

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      const payload = {
        teamId,
        title: title.trim(),
        description: description.trim(),
        category: category.trim(),
        questions: orderedQuestions.map((question, index) => ({
          prompt: question.prompt.trim(),
          type: question.type,
          order: index + 1,
          options: requiresOptions(question.type) ? normalizeOptions(question.optionsText) : [],
        })),
      };

      const result = await createBrandSurvey(payload);
      setSuccess(result.message ?? 'Brand survey created. Redirecting…');
      setTimeout(() => navigate('/onboarding/brand-survey'), 900);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors[0] ?? err.message ?? 'Unable to create survey.');
      } else {
        setError('Unexpected error while creating the survey.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="brand-survey-create">
      <PageHeader
        eyebrow="Onboarding"
        title="Create brand survey"
        description="Draft questions and publish a template your team can reuse for onboarding flows."
        actions={
          <button type="button" className="button button--secondary" onClick={() => navigate(-1)}>
            Back
          </button>
        }
      />

      <section className="brand-survey-create__form-wrapper">
        <form className="brand-survey-create__form" onSubmit={handleSubmit}>
          <label className="brand-survey-create__field">
            <span>Team ID</span>
            <input
              type="text"
              value={teamId}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setTeamId(event.target.value)}
              placeholder="00000000-0000-0000-0000-000000000000"
              required
            />
          </label>

          <label className="brand-survey-create__field">
            <span>Survey title</span>
            <input
              type="text"
              value={title}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setTitle(event.target.value)}
              placeholder="Voice & tone discovery"
              required
            />
          </label>

          <label className="brand-survey-create__field">
            <span>Description</span>
            <textarea
              rows={4}
              value={description}
              onChange={(event: ChangeEvent<HTMLTextAreaElement>) => setDescription(event.target.value)}
              placeholder="Short summary to help your team understand when to use this survey."
            />
          </label>

          <label className="brand-survey-create__field">
            <span>Category</span>
            <input
              type="text"
              value={category}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setCategory(event.target.value)}
              placeholder="Onboarding"
              required
            />
          </label>

          <section className="brand-survey-create__questions" aria-label="Survey questions">
            <header>
              <h3>Questions</h3>
              <p>Configure the prompts and response type for onboarding participants.</p>
            </header>

            {orderedQuestions.map((question, index) => (
              <article key={index} className="brand-survey-create__question">
                <div className="brand-survey-create__question-header">
                  <h4>Question {index + 1}</h4>
                  <button
                    type="button"
                    className="button button--secondary brand-survey-create__remove"
                    onClick={() => handleRemoveQuestion(index)}
                    disabled={orderedQuestions.length === 1}
                  >
                    Remove
                  </button>
                </div>

                <label className="brand-survey-create__field">
                  <span>Prompt</span>
                  <textarea
                    rows={3}
                    value={question.prompt}
                    onChange={(event) => handleQuestionChange(index, 'prompt', event.target.value)}
                    placeholder="Tell us about your brand voice."
                    required
                  />
                </label>

                <label className="brand-survey-create__field">
                  <span>Response type</span>
                  <select
                    value={question.type}
                    onChange={(event) => handleQuestionChange(index, 'type', event.target.value)}
                  >
                    {questionTypeOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>

                {requiresOptions(question.type) ? (
                  <label className="brand-survey-create__field">
                    <span>Options (one per line)</span>
                    <textarea
                      rows={3}
                      value={question.optionsText}
                      onChange={(event) => handleQuestionChange(index, 'optionsText', event.target.value)}
                      placeholder={"Friendly\nPlayful\nProfessional"}
                    />
                  </label>
                ) : null}
              </article>
            ))}

            <button type="button" className="button button--secondary" onClick={handleAddQuestion}>
              Add question
            </button>
          </section>

          {error ? (
            <div className="brand-survey-create__alert brand-survey-create__alert--error" role="alert">
              {error}
            </div>
          ) : null}

          {success ? (
            <div className="brand-survey-create__alert brand-survey-create__alert--success" role="status">
              {success}
            </div>
          ) : null}

          <div className="brand-survey-create__actions">
            <button type="submit" className="button button--primary" disabled={isSubmitting}>
              {isSubmitting ? 'Creating…' : 'Create survey'}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
};

export default BrandSurveyCreatePage;
