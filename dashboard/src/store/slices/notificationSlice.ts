import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  title: string;
  message: string;
  timestamp: string;
  isRead: boolean;
  data?: any;
}

interface NotificationState {
  notifications: Notification[];
  unreadCount: number;
  isConnected: boolean;
}

const initialState: NotificationState = {
  notifications: [],
  unreadCount: 0,
  isConnected: false,
};

const notificationSlice = createSlice({
  name: 'notifications',
  initialState,
  reducers: {
    addNotification: (state, action: PayloadAction<Omit<Notification, 'id' | 'timestamp' | 'isRead'>>) => {
      const notification: Notification = {
        id: Math.random().toString(),
        timestamp: new Date().toISOString(),
        isRead: false,
        ...action.payload,
      };
      state.notifications.unshift(notification);
      state.unreadCount += 1;
    },
    markAsRead: (state, action: PayloadAction<string>) => {
      const notification = state.notifications.find(n => n.id === action.payload);
      if (notification && !notification.isRead) {
        notification.isRead = true;
        state.unreadCount -= 1;
      }
    },
    markAllAsRead: (state) => {
      state.notifications.forEach(n => n.isRead = true);
      state.unreadCount = 0;
    },
    removeNotification: (state, action: PayloadAction<string>) => {
      const index = state.notifications.findIndex(n => n.id === action.payload);
      if (index !== -1) {
        const notification = state.notifications[index];
        if (!notification.isRead) {
          state.unreadCount -= 1;
        }
        state.notifications.splice(index, 1);
      }
    },
    clearNotifications: (state) => {
      state.notifications = [];
      state.unreadCount = 0;
    },
    setConnectionStatus: (state, action: PayloadAction<boolean>) => {
      state.isConnected = action.payload;
    },
    // Handle real-time notifications from SignalR
    handleContentApprovalUpdate: (state, action: PayloadAction<any>) => {
      const data = action.payload;
      const notification: Notification = {
        id: Math.random().toString(),
        type: data.Type === 'success' ? 'success' : 'warning',
        title: 'Content Approval Update',
        message: data.Message,
        timestamp: new Date().toISOString(),
        isRead: false,
        data,
      };
      state.notifications.unshift(notification);
      state.unreadCount += 1;
    },
    handleContentPublished: (state, action: PayloadAction<any>) => {
      const data = action.payload;
      const notification: Notification = {
        id: Math.random().toString(),
        type: 'success',
        title: 'Content Published',
        message: data.Message,
        timestamp: new Date().toISOString(),
        isRead: false,
        data,
      };
      state.notifications.unshift(notification);
      state.unreadCount += 1;
    },
    handleContentScheduled: (state, action: PayloadAction<any>) => {
      const data = action.payload;
      const notification: Notification = {
        id: Math.random().toString(),
        type: 'info',
        title: 'Content Scheduled',
        message: data.Message,
        timestamp: new Date().toISOString(),
        isRead: false,
        data,
      };
      state.notifications.unshift(notification);
      state.unreadCount += 1;
    },
  },
});

export const {
  addNotification,
  markAsRead,
  markAllAsRead,
  removeNotification,
  clearNotifications,
  setConnectionStatus,
  handleContentApprovalUpdate,
  handleContentPublished,
  handleContentScheduled,
} = notificationSlice.actions;

export default notificationSlice.reducer;