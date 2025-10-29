import { createBrowserRouter } from 'react-router-dom';
import DashboardLayout from '../layouts/DashboardLayout';
import OverviewPage from '../pages/Overview/OverviewPage';
import BrandSurveyWizardPage from '../pages/BrandSurvey/BrandSurveyWizardPage';
import BrandSurveyCreatePage from '../pages/BrandSurvey/BrandSurveyCreatePage';
import BrandSurveyImportPage from '../pages/BrandSurvey/BrandSurveyImportPage';
import ContentPipelinePage from '../pages/ContentPipeline/ContentPipelinePage';
import ContentGenerationPage from '../pages/ContentGeneration/ContentGenerationPage';
import NotFoundPage from '../pages/NotFound/NotFoundPage';
import ProtectedRoute from '../components/auth/ProtectedRoute';
import LoginPage from '../pages/Auth/LoginPage';
import SignUpPage from '../pages/Auth/SignUpPage';

export const appRouter = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <SignUpPage />,
  },
  // Legacy auth routes for compatibility
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
            path: 'dashboard',
            element: <OverviewPage />,
          },
          {
            path: 'surveys',
            element: <BrandSurveyWizardPage />,
          },
          {
            path: 'surveys/create',
            element: <BrandSurveyCreatePage />,
          },
          {
            path: 'surveys/import',
            element: <BrandSurveyImportPage />,
          },
          {
            path: 'content',
            element: <ContentGenerationPage />,
          },
          {
            path: 'approvals',
            element: <ContentPipelinePage />, // Temporarily using existing page
          },
          {
            path: 'scheduler',
            element: <ContentPipelinePage />, // Temporarily using existing page
          },
          // Legacy routes for compatibility
          {
            path: 'onboarding/brand-survey',
            element: <BrandSurveyWizardPage />,
          },
          {
            path: 'onboarding/brand-survey/create',
            element: <BrandSurveyCreatePage />,
          },
          {
            path: 'onboarding/brand-survey/import',
            element: <BrandSurveyImportPage />,
          },
          {
            path: 'content/pipeline',
            element: <ContentPipelinePage />,
          },
          {
            path: 'content/generate',
            element: <ContentGenerationPage />,
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
