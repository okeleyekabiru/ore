import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import type { Location } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { ApiError } from '../../types/api';
import './LoginPage.css';

interface LocationState {
  from?: Location;
}

const LoginPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isAuthenticated, isAuthenticating, isHydrating } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);

  const state = (location.state as LocationState | undefined) ?? {};
  const redirectTo = state.from?.pathname ?? '/';

  useEffect(() => {
    if (isAuthenticated && !isHydrating) {
      navigate(redirectTo, { replace: true });
    }
  }, [isAuthenticated, isHydrating, navigate, redirectTo]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    try {
      await login({ email, password });
      navigate(redirectTo, { replace: true });
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors[0] ?? err.message ?? 'Unable to sign in.');
      } else {
        setError('Unexpected error during sign in.');
      }
    }
  };

  return (
    <div className="login-page">
      <section className="login-card">
        <div className="login-card__header">
          <h1>Sign in to Ore</h1>
          <p>Enter the credentials you created via the API to access the dashboard.</p>
        </div>

        {error ? (
          <div className="login-card__error" role="alert">
            {error}
          </div>
        ) : null}

        <form className="login-form" onSubmit={handleSubmit}>
          <label className="login-form__field">
            <span>Email</span>
            <input
              type="email"
              name="email"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </label>

          <label className="login-form__field">
            <span>Password</span>
            <input
              type="password"
              name="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </label>

          <button
            type="submit"
            className="button button--primary login-form__submit"
            disabled={isAuthenticating || isHydrating}
          >
            {isAuthenticating ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        <footer className="login-card__footer">
          <p>
            Need an account?{' '}
            <Link to="/auth/register" className="login-card__link">
              Create one now
            </Link>
            .
          </p>
          <Link to="/" className="login-card__back-link">
            Back to dashboard overview
          </Link>
        </footer>
      </section>
    </div>
  );
};

export default LoginPage;
