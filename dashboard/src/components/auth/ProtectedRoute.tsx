import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

const ProtectedRoute = () => {
  const location = useLocation();
  const { isAuthenticated, isHydrating } = useAuth();

  if (isHydrating) {
    return (
      <div className="route-loader" role="status" aria-live="polite">
        Preparing your workspace…
      </div>
    );
  }

  if (isAuthenticated) {
    return <Outlet />;
  }

  return <Navigate to="/auth/login" replace state={{ from: location }} />;
};

export default ProtectedRoute;
