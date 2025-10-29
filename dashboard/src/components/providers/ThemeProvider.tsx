import { useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState } from '../../store';
import { setDarkMode } from '../../store/slices/uiSlice';

interface ThemeProviderProps {
  children: React.ReactNode;
}

export const ThemeProvider = ({ children }: ThemeProviderProps) => {
  const { isDarkMode } = useSelector((state: RootState) => state.ui);
  const dispatch = useDispatch();

  useEffect(() => {
    // Apply dark mode class to document
    if (isDarkMode) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [isDarkMode]);

  useEffect(() => {
    // Listen for system theme changes
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const handleChange = (e: MediaQueryListEvent) => {
      // Only update if user hasn't manually set a preference
      const hasPreference = localStorage.getItem('darkMode') !== null;
      if (!hasPreference) {
        dispatch(setDarkMode(e.matches));
      }
    };

    mediaQuery.addEventListener('change', handleChange);
    
    // Set initial theme if no preference is saved
    const hasPreference = localStorage.getItem('darkMode') !== null;
    if (!hasPreference) {
      dispatch(setDarkMode(mediaQuery.matches));
    }

    return () => mediaQuery.removeEventListener('change', handleChange);
  }, [dispatch]);

  return <>{children}</>;
};