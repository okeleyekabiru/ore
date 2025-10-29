import { configureStore } from '@reduxjs/toolkit';
import authSlice from './slices/authSlice';
import uiSlice from './slices/uiSlice';
import surveySlice from './slices/surveySlice';
import contentSlice from './slices/contentSlice';
import approvalSlice from './slices/approvalSlice';
import notificationSlice from './slices/notificationSlice';

export const store = configureStore({
  reducer: {
    auth: authSlice,
    ui: uiSlice,
    surveys: surveySlice,
    content: contentSlice,
    approvals: approvalSlice,
    notifications: notificationSlice,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;