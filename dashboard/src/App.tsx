import { RouterProvider } from 'react-router-dom';
import { appRouter } from './routes/AppRouter';
import { AuthProvider } from './contexts/AuthContext';

const App = () => {
  return (
    <AuthProvider>
      <RouterProvider router={appRouter} />
    </AuthProvider>
  );
};

export default App;
