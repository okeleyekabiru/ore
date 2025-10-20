import { useState, type ChangeEvent, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { ApiError } from '../../types/api';
import * as authService from '../../services/authService';
import type { RegisterRequest } from '../../types/auth';
import { ROLE_TYPES } from '../../types/auth';
import './SignUpPage.css';

type AccountType = 'individual' | 'team';

type FormState = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  teamName: string;
  voice: string;
  tone: string;
  audience: string;
  goals: string;
  competitors: string;
  keywords: string;
};

const TEAM_ROLE_OPTIONS = [
  { value: ROLE_TYPES.Admin, label: 'Admin' },
  { value: ROLE_TYPES.SocialMediaManager, label: 'Social Media Manager' },
  { value: ROLE_TYPES.ContentCreator, label: 'Content Creator' },
  { value: ROLE_TYPES.Approver, label: 'Approver' },
];

const initialFormState: FormState = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  teamName: '',
  voice: '',
  tone: '',
  audience: '',
  goals: '',
  competitors: '',
  keywords: '',
};

const SignUpPage = () => {
  const [form, setForm] = useState<FormState>(initialFormState);
  const [accountType, setAccountType] = useState<AccountType>('individual');
  const [teamRole, setTeamRole] = useState<RegisterRequest['role']>(ROLE_TYPES.SocialMediaManager);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const isIndividual = accountType === 'individual';

  const handleChange = (field: keyof FormState) => (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm((prev) => ({
      ...prev,
      [field]: event.target.value,
    }));
  };

  const handleAccountTypeChange = (event: ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value as AccountType;
    setAccountType(value);
  };

  const handleTeamRoleChange = (event: ChangeEvent<HTMLSelectElement>) => {
    setTeamRole(Number(event.target.value) as RegisterRequest['role']);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setSuccess(null);

    if (!form.firstName.trim() || !form.lastName.trim()) {
      setError('First and last name are required.');
      return;
    }

    if (!isIndividual && !form.teamName.trim()) {
      setError('Team accounts require a team name.');
      return;
    }

    if (!form.voice.trim()) {
      setError('Share a few words about your brand voice to personalize the workspace.');
      return;
    }

    if (!form.tone.trim()) {
      setError('Describe the tone your brand prefers.');
      return;
    }

    const parseKeywords = (value: string) =>
      value
        .split(/\r?\n|,/)
        .map((keyword) => keyword.trim())
        .filter((keyword) => keyword.length > 0);

    const payload: RegisterRequest = {
      email: form.email.trim(),
      password: form.password,
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      role: isIndividual ? ROLE_TYPES.Individual : teamRole,
      teamName: isIndividual ? null : form.teamName.trim(),
      isIndividual,
      brandSurvey: {
        voice: form.voice.trim(),
        tone: form.tone.trim(),
        audience: form.audience.trim(),
        goals: form.goals.trim(),
        competitors: form.competitors.trim(),
        keywords: parseKeywords(form.keywords),
      },
    };

    setIsSubmitting(true);
    try {
      await authService.register(payload);
      setSuccess('Account created successfully. You can now sign in.');
      setForm((prev) => ({ ...initialFormState, email: prev.email }));
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors[0] ?? err.message ?? 'Unable to create the account.');
      } else {
        setError('Unexpected error while creating the account.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="signup-page">
      <section className="signup-card">
        <div className="signup-card__header">
          <h1>Create your Ore account</h1>
          <p>Register a new dashboard user. Individual accounts create a personal workspace; teams can invite others later.</p>
        </div>

        {error ? (
          <div className="signup-card__error" role="alert">
            {error}
          </div>
        ) : null}

        {success ? (
          <div className="signup-card__success" role="status">
            {success}{' '}
            <Link to="/auth/login">Sign in</Link>
          </div>
        ) : null}

        <form className="signup-form" onSubmit={handleSubmit} noValidate>
          <fieldset className="signup-form__fieldset">
            <legend>Account type</legend>
            <label className="signup-form__radio">
              <input
                type="radio"
                name="accountType"
                value="individual"
                checked={isIndividual}
                onChange={handleAccountTypeChange}
              />
              <span>Individual</span>
            </label>
            <label className="signup-form__radio">
              <input
                type="radio"
                name="accountType"
                value="team"
                checked={!isIndividual}
                onChange={handleAccountTypeChange}
              />
              <span>Team</span>
            </label>
          </fieldset>

          <div className="signup-form__row">
            <label className="signup-form__field">
              <span>First name</span>
              <input
                type="text"
                name="firstName"
                autoComplete="given-name"
                value={form.firstName}
                onChange={handleChange('firstName')}
                required
              />
            </label>
            <label className="signup-form__field">
              <span>Last name</span>
              <input
                type="text"
                name="lastName"
                autoComplete="family-name"
                value={form.lastName}
                onChange={handleChange('lastName')}
                required
              />
            </label>
          </div>

          <label className="signup-form__field">
            <span>Email</span>
            <input
              type="email"
              name="email"
              autoComplete="email"
              value={form.email}
              onChange={handleChange('email')}
              required
            />
          </label>

          <label className="signup-form__field">
            <span>Password</span>
            <input
              type="password"
              name="password"
              autoComplete="new-password"
              value={form.password}
              onChange={handleChange('password')}
              required
              minLength={8}
            />
          </label>

          {!isIndividual ? (
            <>
              <label className="signup-form__field">
                <span>Role</span>
                <select name="role" value={teamRole} onChange={handleTeamRoleChange}>
                  {TEAM_ROLE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>

              <label className="signup-form__field">
                <span>Team name</span>
                <input
                  type="text"
                  name="teamName"
                  value={form.teamName}
                  onChange={handleChange('teamName')}
                  required
                />
              </label>
            </>
          ) : (
            <p className="signup-form__helper">
              Individual accounts automatically use the individual role and create a private team for your brand.
            </p>
          )}

          <section className="signup-form__section" aria-label="Brand onboarding details">
            <header className="signup-form__section-header">
              <h2>Brand onboarding</h2>
              <p>Provide quick context for your voice, tone, and goals so the workspace can tailor recommendations.</p>
            </header>

            <label className="signup-form__field">
              <span>Brand voice</span>
              <input
                type="text"
                name="voice"
                value={form.voice}
                onChange={handleChange('voice')}
                required
              />
            </label>

            <label className="signup-form__field">
              <span>Tone</span>
              <input
                type="text"
                name="tone"
                value={form.tone}
                onChange={handleChange('tone')}
                required
              />
            </label>

            <div className="signup-form__row">
              <label className="signup-form__field">
                <span>Audience</span>
                <input
                  type="text"
                  name="audience"
                  value={form.audience}
                  onChange={handleChange('audience')}
                  placeholder="E.g. Growth-stage SaaS founders"
                />
              </label>

              <label className="signup-form__field">
                <span>Primary goals</span>
                <textarea
                  rows={3}
                  name="goals"
                  value={form.goals}
                  onChange={handleChange('goals')}
                  placeholder="Summarize what success looks like for your brand."
                />
              </label>
            </div>

            <label className="signup-form__field">
              <span>Competitors</span>
              <textarea
                rows={2}
                name="competitors"
                value={form.competitors}
                onChange={handleChange('competitors')}
                placeholder="List any brands you track."
              />
            </label>

            <label className="signup-form__field">
              <span>Keywords (comma or newline separated)</span>
              <textarea
                rows={2}
                name="keywords"
                value={form.keywords}
                onChange={handleChange('keywords')}
                placeholder="Friendly, Human, Insightful"
              />
            </label>
          </section>

          <button type="submit" className="button button--primary signup-form__submit" disabled={isSubmitting}>
            {isSubmitting ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        <footer className="signup-card__footer">
          <p>
            Already have an account?{' '}
            <Link to="/auth/login" className="signup-card__link">
              Sign in instead
            </Link>
            .
          </p>
        </footer>
      </section>
    </div>
  );
};

export default SignUpPage;
