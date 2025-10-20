import type { ReactElement } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

interface RequireRoleProps {
  allowedRoles: number[];
  children: ReactElement;
}

const RequireRole = ({ allowedRoles, children }: RequireRoleProps): ReactElement => {
  const location = useLocation();
  const { user, isHydrating, isAuthenticated } = useAuth();

  if (isHydrating) {
    return (
      <div className="route-loader" role="status" aria-live="polite">
        Preparing your workspace…
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/auth/login" replace state={{ from: location }} />;
  }

  if (!user.role || !allowedRoles.includes(user.role)) {
    return <Navigate to="/onboarding/brand-survey" replace />;
  }

  return children;
};

export default RequireRole;
