import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageHeader from '../../components/common/PageHeader';
import { ApiError } from '../../types/api';
import { listBrandSurveys } from '../../services/brandSurveyService';
import type { BrandSurveySummary } from '../../types/brandSurvey';
import './BrandSurveyWizardPage.css';

type LoadState = 'idle' | 'loading' | 'success' | 'error';

const BrandSurveyWizardPage = () => {
  const navigate = useNavigate();
  const [surveys, setSurveys] = useState<BrandSurveySummary[]>([]);
  const [state, setState] = useState<LoadState>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    const fetchSurveys = async () => {
      setState('loading');
      setErrorMessage(null);

      try {
        const result = await listBrandSurveys({ includeInactive: true, signal: controller.signal });
        setSurveys(result.data);
        setState('success');
      } catch (error) {
        if (error instanceof ApiError) {
          if (error.status === 401) {
            setErrorMessage('You need to authenticate before loading brand surveys. Log in to the API and refresh the page.');
          } else {
            setErrorMessage(error.errors[0] ?? error.message);
          }
        } else if (error instanceof DOMException && error.name === 'AbortError') {
          // ignore aborts triggered during unmount
        } else {
          setErrorMessage('Unable to load brand surveys right now.');
        }

        setState('error');
      }
    };

    fetchSurveys();

    return () => controller.abort();
  }, []);

  const renderContent = () => {
    if (state === 'loading') {
      return (
        <div className="brand-survey__loading" role="status" aria-live="polite">
          Loading latest brand surveys...
        </div>
      );
    }

    if (state === 'error') {
      return (
        <div className="brand-survey__error" role="alert">
          {errorMessage ?? 'Something went wrong while contacting the API.'}
        </div>
      );
    }

    if (surveys.length === 0) {
      return (
        <div className="brand-survey__empty">
          <h3>No brand surveys yet</h3>
          <p>Kick off onboarding by creating your first brand survey template in the API.</p>
        </div>
      );
    }

    return (
      <section className="brand-survey__list" aria-label="Configured brand surveys">
        {surveys.map((survey) => (
          <article key={survey.id} className="card brand-survey__card">
            <header className="brand-survey__card-header">
              <div>
                <h3>{survey.title}</h3>
                <p>{survey.description}</p>
              </div>
              <span className={survey.isActive ? 'badge badge--success' : 'badge badge--muted'}>
                {survey.isActive ? 'Active' : 'Inactive'}
              </span>
            </header>
            <dl>
              <div>
                <dt>Question count</dt>
                <dd>{survey.questionCount}</dd>
              </div>
              <div>
                <dt>Team</dt>
                <dd>{survey.teamId}</dd>
              </div>
              <div>
                <dt>Last updated</dt>
                <dd>{survey.modifiedOnUtc ?? survey.createdOnUtc}</dd>
              </div>
            </dl>
          </article>
        ))}
      </section>
    );
  };

  return (
    <div className="brand-survey">
      <PageHeader
        eyebrow="Onboarding"
        title="Brand survey templates"
        description="Fetches the latest surveys from the API so you can track onboarding coverage and jump into the wizard."
        actions={
          <div className="brand-survey__actions">
            <button
              type="button"
              className="button button--secondary"
              onClick={() => navigate('/onboarding/brand-survey/import')}
            >
              Import template
            </button>
            <button
              type="button"
              className="button button--primary"
              onClick={() => navigate('/onboarding/brand-survey/create')}
            >
              Create survey
            </button>
          </div>
        }
      />

      {renderContent()}
    </div>
  );
};

export default BrandSurveyWizardPage;
