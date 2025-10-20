import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageHeader from '../../components/common/PageHeader';
import { useAuth } from '../../contexts/AuthContext';
import { createBrandSurvey } from '../../services/brandSurveyService';
import { ApiError } from '../../types/api';
import { SurveyQuestionType } from '../../types/brandSurvey';
import type { SurveyQuestionType as SurveyQuestionTypeValue } from '../../types/brandSurvey';
import './BrandSurveyImportPage.css';

type ImportStatus = 'idle' | 'importing' | 'success' | 'error';

interface TemplateQuestionInput {
  prompt?: unknown;
  type?: unknown;
  order?: unknown;
  options?: unknown;
}

interface TemplateInput {
  teamId?: unknown;
  title?: unknown;
  description?: unknown;
  category?: unknown;
  questions?: unknown;
}

const normalizeOptions = (raw: unknown) => {
  if (Array.isArray(raw)) {
    return raw.map((item) => (typeof item === 'string' ? item.trim() : String(item))).filter((item) => item.length > 0);
  }

  if (typeof raw === 'string') {
    return raw
      .split(/\r?\n|,/)
      .map((item) => item.trim())
      .filter((item) => item.length > 0);
  }

  return [];
};

const SURVEY_TYPE_VALUES = Object.values(SurveyQuestionType) as SurveyQuestionTypeValue[];

const resolveQuestionType = (value: unknown): SurveyQuestionTypeValue => {
  if (typeof value === 'number') {
    if ((SURVEY_TYPE_VALUES as number[]).includes(value)) {
      return value as SurveyQuestionTypeValue;
    }
  }

  if (typeof value === 'string') {
    const normalized = value.toLowerCase().replace(/[^a-z]/g, '');
    const map: Record<string, SurveyQuestionTypeValue> = {
      text: SurveyQuestionType.Text,
      textarea: SurveyQuestionType.TextArea,
      longform: SurveyQuestionType.TextArea,
      singlechoice: SurveyQuestionType.SingleChoice,
      multichoice: SurveyQuestionType.MultiChoice,
      multiplechoice: SurveyQuestionType.MultiChoice,
      scale: SurveyQuestionType.Scale,
    };

    const resolved = map[normalized];
    if (resolved) {
      return resolved;
    }
  }

  throw new Error(`Unsupported question type: ${String(value)}`);
};

const parseTemplate = (template: unknown) => {
  if (!template || typeof template !== 'object') {
    throw new Error('Template must be a JSON object.');
  }

  const input = template as TemplateInput;

  const title = typeof input.title === 'string' && input.title.trim().length > 0 ? input.title.trim() : null;
  if (!title) {
    throw new Error('Template is missing a title.');
  }

  const description = typeof input.description === 'string' ? input.description.trim() : '';

  const category = typeof input.category === 'string' && input.category.trim().length > 0 ? input.category.trim() : null;
  if (!category) {
    throw new Error('Template is missing a category. Add a descriptive category such as "Onboarding".');
  }

  if (!Array.isArray(input.questions) || input.questions.length === 0) {
    throw new Error('Template requires at least one question.');
  }

  const questions = input.questions.map((item, index) => {
    if (!item || typeof item !== 'object') {
      throw new Error(`Question ${index + 1} is invalid.`);
    }

    const question = item as TemplateQuestionInput;
    const prompt = typeof question.prompt === 'string' ? question.prompt.trim() : '';
    if (!prompt) {
      throw new Error(`Question ${index + 1} is missing a prompt.`);
    }

    const type = resolveQuestionType(question.type ?? SurveyQuestionType.Text);
    const order = typeof question.order === 'number' ? question.order : index + 1;
    const options = normalizeOptions(question.options ?? []);

    return {
      prompt,
      type,
      order,
      options,
    };
  });

  const sortedQuestions = [...questions].sort((a, b) => a.order - b.order).map((question, index) => ({
    ...question,
    order: index + 1,
  }));

  const teamId = typeof input.teamId === 'string' ? input.teamId.trim() : '';

  return {
    teamId,
    title,
    description,
    category,
    questions: sortedQuestions,
  };
};

const BrandSurveyImportPage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [status, setStatus] = useState<ImportStatus>('idle');
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const triggerFilePicker = () => fileInputRef.current?.click();

  const handleFileChange = () => {
    const file = fileInputRef.current?.files?.[0] ?? null;
    setSelectedFile(file);
    setMessage(null);
    setError(null);
  };

  const handleImport = async () => {
    if (!selectedFile) {
      setError('Choose a JSON template before importing.');
      return;
    }

    setStatus('importing');
    setError(null);
    setMessage(null);

    try {
      const fileContents = await selectedFile.text();
      const parsed = JSON.parse(fileContents) as unknown;
      const payload = parseTemplate(parsed);

      const resolvedTeamId = payload.teamId || user?.teamId;
      if (!resolvedTeamId) {
        throw new Error('Template is missing a teamId and your session is not associated with a team.');
      }

      const result = await createBrandSurvey({
        ...payload,
        teamId: resolvedTeamId,
      });

      setStatus('success');
      setMessage(result.message ?? 'Template imported. Redirecting…');
      setTimeout(() => navigate('/onboarding/brand-survey'), 900);
    } catch (err) {
      setStatus('error');
      if (err instanceof ApiError) {
        setError(err.errors[0] ?? err.message ?? 'Unable to import template.');
      } else if (err instanceof SyntaxError) {
        setError('Template is not valid JSON.');
      } else if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Unexpected error during import.');
      }
    }
  };

  return (
    <div className="brand-survey-import">
      <PageHeader
        eyebrow="Onboarding"
        title="Import survey template"
        description="Upload a JSON template exported from another workspace."
        actions={
          <button type="button" className="button button--secondary" onClick={() => navigate(-1)}>
            Back
          </button>
        }
      />

      <section className="brand-survey-import__panel">
        <input
          ref={fileInputRef}
          type="file"
          accept="application/json"
          onChange={handleFileChange}
          className="brand-survey-import__input"
        />

        <div className="brand-survey-import__body">
          <p>Select a template to prepare it for import into Ore.</p>
          <p className="brand-survey-import__hint">
            Need a starting point?{' '}
            <a href="/samples/brand-survey-template.json" download>
              Download the sample JSON file
            </a>{' '}
            and update the questions before importing.
          </p>
          {selectedFile ? <span className="brand-survey-import__file">Selected: {selectedFile.name}</span> : null}
          <div className="brand-survey-import__actions">
            <button type="button" className="button button--secondary" onClick={triggerFilePicker}>
              Choose template
            </button>
            <button
              type="button"
              className="button button--primary"
              onClick={handleImport}
              disabled={status === 'importing'}
            >
              {status === 'importing' ? 'Importing…' : 'Import'}
            </button>
          </div>
        </div>

        {error ? (
          <div className="brand-survey-import__alert brand-survey-import__alert--error" role="alert">
            {error}
          </div>
        ) : null}

        {message ? (
          <div className="brand-survey-import__alert brand-survey-import__alert--success" role="status">
            {message}
          </div>
        ) : null}
      </section>
    </div>
  );
};

export default BrandSurveyImportPage;
