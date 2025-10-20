import { useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import PageHeader from '../../components/common/PageHeader';
import { useAuth } from '../../contexts/AuthContext';
import { contentGenerationService } from '../../services/contentGenerationService';
import { ApiError } from '../../types/api';
import { PLATFORM_CHOICES, type GeneratedContentSuggestion } from '../../types/contentGeneration';
import './ContentGenerationPage.css';

const ContentGenerationPage = () => {
  const { user } = useAuth();
  const teamId = user?.teamId ?? null;

  const [topic, setTopic] = useState('');
  const [tone, setTone] = useState('Confident');
  const [platform, setPlatform] = useState(PLATFORM_CHOICES[0]?.value ?? 'LinkedIn');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [result, setResult] = useState<GeneratedContentSuggestion | null>(null);

  const isReady = useMemo(() => Boolean(teamId), [teamId]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!teamId) {
      setError('Team context missing. Complete onboarding to capture brand details first.');
      return;
    }

    const trimmedTopic = topic.trim();
    const trimmedTone = tone.trim();

    if (!trimmedTopic) {
      setError('Add a topic so the assistant knows what to write about.');
      return;
    }

    if (!trimmedTone) {
      setError('Provide a tone to reinforce brand voice.');
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setSuccessMessage(null);

    try {
      const response = await contentGenerationService.generateContent({
        brandId: teamId,
        topic: trimmedTopic,
        tone: trimmedTone,
        platform,
      });

      setResult(response.data);
      setSuccessMessage(response.message ?? 'Draft suggestion ready. Fine-tune before publishing.');
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors[0] ?? err.message ?? 'Unable to generate right now.');
      } else {
        setError('Unexpected error occurred while requesting content.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetResult = () => {
    setResult(null);
    setSuccessMessage(null);
  };

  return (
    <div className="content-generation">
      <PageHeader
        eyebrow="Assist"
        title="Content generator"
        description="Draft captions and creative direction tuned to your brand survey response."
      />

      {!isReady ? (
        <div className="content-generation__alert" role="alert">
          Finish onboarding your brand so we know which voice profile to apply before generating content.
        </div>
      ) : null}

      <section className="content-generation__panel" aria-labelledby="content-generator-heading">
        <header className="content-generation__panel-header">
          <div>
            <h2 id="content-generator-heading">Request a suggestion</h2>
            <p>Pick a channel, set the tone, and describe the topic. We will blend in your brand survey data.</p>
          </div>
          {successMessage ? <span className="content-generation__status">{successMessage}</span> : null}
        </header>

        <form className="content-generation__form" onSubmit={handleSubmit}>
          <div className="content-generation__field">
            <label htmlFor="content-generator-topic">Topic</label>
            <textarea
              id="content-generator-topic"
              rows={4}
              value={topic}
              onChange={(event) => {
                if (result) {
                  resetResult();
                }
                setTopic(event.target.value);
              }}
              placeholder="Introduce our spring feature launch to mid-market marketing leads."
              disabled={!isReady || isSubmitting}
            />
            <span className="content-generation__hint">Describe the hook, offer, or story to highlight.</span>
          </div>

          <div className="content-generation__grid">
            <label className="content-generation__field" htmlFor="content-generator-tone">
              <span>Tone</span>
              <input
                id="content-generator-tone"
                type="text"
                value={tone}
                onChange={(event) => {
                  if (result) {
                    resetResult();
                  }
                  setTone(event.target.value);
                }}
                placeholder="Confident, approachable, witty"
                disabled={!isReady || isSubmitting}
              />
            </label>

            <label className="content-generation__field" htmlFor="content-generator-platform">
              <span>Platform</span>
              <select
                id="content-generator-platform"
                value={platform}
                onChange={(event) => {
                  if (result) {
                    resetResult();
                  }
                  setPlatform(event.target.value);
                }}
                disabled={!isReady || isSubmitting}
              >
                {PLATFORM_CHOICES.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
          </div>

          {error ? (
            <div className="content-generation__error" role="alert">
              {error}
            </div>
          ) : null}

          <div className="content-generation__actions">
            <button type="submit" className="button button--primary" disabled={!isReady || isSubmitting}>
              {isSubmitting ? 'Generating…' : 'Generate suggestion'}
            </button>
            {result ? (
              <button
                type="button"
                className="button button--secondary"
                onClick={resetResult}
                disabled={isSubmitting}
              >
                Start over
              </button>
            ) : null}
          </div>
        </form>
      </section>

      {result ? (
        <section className="content-generation__output" aria-live="polite">
          <header>
            <h2>Suggested copy</h2>
            <p>Review and tailor before pushing to the pipeline.</p>
          </header>
          <article className="content-generation__card">
            <h3>Caption</h3>
            <p>{result.caption}</p>
          </article>
          <article className="content-generation__card">
            <h3>Hashtags</h3>
            {(result.hashtags ?? []).length > 0 ? (
              <ul>
                {(result.hashtags ?? []).map((tag) => (
                  <li key={tag}>{tag}</li>
                ))}
              </ul>
            ) : (
              <p className="content-generation__placeholder">No hashtags returned. Try broadening the topic.</p>
            )}
          </article>
          <article className="content-generation__card">
            <h3>Visual direction</h3>
            <p>{result.imageIdea}</p>
          </article>
        </section>
      ) : (
        <section className="content-generation__empty">
          <h2>Preview queue</h2>
          <p>
            Once generated, we will show caption, hashtags, and a visual direction inspired by your brand voice. Use the
            pipeline shortcut to move promising drafts straight into production.
          </p>
        </section>
      )}
    </div>
  );
};

export default ContentGenerationPage;
