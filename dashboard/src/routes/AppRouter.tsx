import { createBrowserRouter } from 'react-router-dom';
import DashboardLayout from '../layouts/DashboardLayout';
import OverviewPage from '../pages/Overview/OverviewPage';
import BrandSurveyWizardPage from '../pages/BrandSurvey/BrandSurveyWizardPage';
import ContentPipelinePage from '../pages/ContentPipeline/ContentPipelinePage';
import NotFoundPage from '../pages/NotFound/NotFoundPage';

export const appRouter = createBrowserRouter([
  {
    path: '/',
    element: <DashboardLayout />,
    errorElement: <NotFoundPage />, // handles unexpected routing errors
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
      {
        path: '*',
        element: <NotFoundPage />,
      },
    ],
  },
]);
