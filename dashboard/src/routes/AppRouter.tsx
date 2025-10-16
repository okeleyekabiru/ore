import { createBrowserRouter } from 'react-router-dom';
import DashboardLayout from '../layouts/DashboardLayout';
import OverviewPage from '../pages/Overview/OverviewPage';
import BrandSurveyWizardPage from '../pages/BrandSurvey/BrandSurveyWizardPage';
import ContentPipelinePage from '../pages/ContentPipeline/ContentPipelinePage';
import NotFoundPage from '../pages/NotFound/NotFoundPage';
import ProtectedRoute from '../components/auth/ProtectedRoute';
import LoginPage from '../pages/Auth/LoginPage';
import SignUpPage from '../pages/Auth/SignUpPage';

export const appRouter = createBrowserRouter([
  {
    path: '/auth/login',
    element: <LoginPage />,
  },
  {
    path: '/auth/register',
    element: <SignUpPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/',
        element: <DashboardLayout />,
        errorElement: <NotFoundPage />,
        children: [
          {
            index: true,
            element: <OverviewPage />,
          },
          {
            path: 'onboarding/brand-survey',
            element: <BrandSurveyWizardPage />,
          },
          {
            path: 'content/pipeline',
            element: <ContentPipelinePage />,
          },
        ],
      },
    ],
  },
  {
    path: '*',
    element: <NotFoundPage />,
  },
]);
