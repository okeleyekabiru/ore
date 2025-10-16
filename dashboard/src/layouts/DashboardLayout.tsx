import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './DashboardLayout.css';

const navItems = [
  {
    label: 'Overview',
    to: '/',
    description: 'Team pulse, queue health, and platform status.',
  },
  {
    label: 'Brand Survey',
    to: '/onboarding/brand-survey',
    description: 'Onboarding wizard and survey progress tracking.',
  },
  {
    label: 'Content Pipeline',
    to: '/content/pipeline',
    description: 'Monitor generation → approval → scheduling flow.',
  },
];

const locationHints: Record<string, string> = {
  '/': 'Multi-channel performance snapshot and quick metrics.',
  '/onboarding/brand-survey': 'Guide new brands through tone, audience, and asset capture.',
  '/content/pipeline': 'Track ideation, approvals, and scheduled posts by channel.',
};

const DashboardLayout = () => {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const hint = locationHints[pathname] ?? 'Navigate using the sidebar to explore modules.';

  const deriveInitials = (fullName?: string, fallback?: string) => {
    if (fullName) {
      const parts = fullName.trim().split(/\s+/);
      const first = parts[0]?.[0];
      const last = parts.length > 1 ? parts[parts.length - 1]?.[0] : undefined;
      const initials = `${first ?? ''}${last ?? ''}`.toUpperCase();
      if (initials) {
        return initials;
      }
    }

    if (fallback) {
      return fallback.slice(0, 2).toUpperCase();
    }

    return 'OR';
  };

  const avatarLabel = deriveInitials(user?.fullName, user?.email);

  const handleLogout = () => {
    logout();
    navigate('/auth/login', { replace: true });
  };

  return (
    <div className="dashboard">
      <aside className="dashboard__sidebar" aria-label="Primary navigation">
        <div className="dashboard__brand">
          <span className="dashboard__brand-mark">ore</span>
          <span className="dashboard__brand-sub">Social Companion</span>
        </div>
        <nav className="dashboard__nav">
          <ul>
            {navItems.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  className={({ isActive }) =>
                    isActive ? 'dashboard__nav-link dashboard__nav-link--active' : 'dashboard__nav-link'
                  }
                  end={item.to === '/'}
                >
                  <span className="dashboard__nav-label">{item.label}</span>
                  <span className="dashboard__nav-description">{item.description}</span>
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>
      </aside>

      <section className="dashboard__main">
        <header className="dashboard__topbar">
          <div className="dashboard__topbar-copy">
            <h1>Command Center</h1>
            <p>{hint}</p>
          </div>
          <div className="dashboard__topbar-user" aria-label="Current user">
            <span className="dashboard__avatar">{avatarLabel}</span>
            <div>
              <span className="dashboard__user-name">{user?.fullName ?? 'Signed in'}</span>
              <span className="dashboard__user-role">{user?.email ?? 'Authenticated access'}</span>
            </div>
            <button type="button" onClick={handleLogout} className="dashboard__logout button button--secondary">
              Sign out
            </button>
          </div>
        </header>

        <main className="dashboard__content">
          <Outlet />
        </main>
      </section>
    </div>
  );
};

export default DashboardLayout;
