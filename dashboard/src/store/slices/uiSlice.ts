import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface UIState {
  isDarkMode: boolean;
  isSidebarCollapsed: boolean;
  isLoading: boolean;
  notifications: {
    show: boolean;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
  };
}

const initialState: UIState = {
  isDarkMode: localStorage.getItem('darkMode') === 'true',
  isSidebarCollapsed: localStorage.getItem('sidebarCollapsed') === 'true',
  isLoading: false,
  notifications: {
    show: false,
    message: '',
    type: 'info',
  },
};

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    toggleDarkMode: (state) => {
      state.isDarkMode = !state.isDarkMode;
      localStorage.setItem('darkMode', state.isDarkMode.toString());
    },
    setDarkMode: (state, action: PayloadAction<boolean>) => {
      state.isDarkMode = action.payload;
      localStorage.setItem('darkMode', state.isDarkMode.toString());
    },
    toggleSidebar: (state) => {
      state.isSidebarCollapsed = !state.isSidebarCollapsed;
      localStorage.setItem('sidebarCollapsed', state.isSidebarCollapsed.toString());
    },
    setSidebarCollapsed: (state, action: PayloadAction<boolean>) => {
      state.isSidebarCollapsed = action.payload;
      localStorage.setItem('sidebarCollapsed', state.isSidebarCollapsed.toString());
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    showNotification: (state, action: PayloadAction<{ message: string; type: 'success' | 'error' | 'info' | 'warning' }>) => {
      state.notifications = {
        show: true,
        message: action.payload.message,
        type: action.payload.type,
      };
    },
    hideNotification: (state) => {
      state.notifications.show = false;
    },
  },
});

export const {
  toggleDarkMode,
  setDarkMode,
  toggleSidebar,
  setSidebarCollapsed,
  setLoading,
  showNotification,
  hideNotification,
} = uiSlice.actions;

export default uiSlice.reducer;