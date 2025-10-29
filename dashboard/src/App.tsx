import { Provider } from 'react-redux';
import { RouterProvider } from 'react-router-dom';
import { store } from './store';
import { appRouter } from './routes/AppRouter';
import { ThemeProvider } from './components/providers/ThemeProvider';
import { NotificationProvider } from './components/providers/NotificationProvider';
import { SignalRProvider } from './components/providers/SignalRProvider';

const App = () => {
  return (
    <Provider store={store}>
      <ThemeProvider>
        <SignalRProvider>
          <NotificationProvider>
            <RouterProvider router={appRouter} />
          </NotificationProvider>
        </SignalRProvider>
      </ThemeProvider>
    </Provider>
  );
};

export default App;
