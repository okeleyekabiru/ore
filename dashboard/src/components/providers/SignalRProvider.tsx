import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import * as signalR from '@microsoft/signalr';
import { 
  setConnectionStatus,
  handleContentApprovalUpdate,
  handleContentPublished,
  handleContentScheduled 
} from '../../store/slices/notificationSlice';

interface SignalRProviderProps {
  children: React.ReactNode;
}

export const SignalRProvider = ({ children }: SignalRProviderProps) => {
  const dispatch = useDispatch();
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    // Create SignalR connection
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5160/hubs/notifications', {
        accessTokenFactory: () => localStorage.getItem('token') || '',
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // Set up event handlers
    connection.on('ContentApprovalUpdate', (data) => {
      dispatch(handleContentApprovalUpdate(data));
    });

    connection.on('ContentPublished', (data) => {
      dispatch(handleContentPublished(data));
    });

    connection.on('ContentScheduled', (data) => {
      dispatch(handleContentScheduled(data));
    });

    // Handle connection state changes
    connection.onclose(() => {
      dispatch(setConnectionStatus(false));
    });

    connection.onreconnecting(() => {
      dispatch(setConnectionStatus(false));
    });

    connection.onreconnected(() => {
      dispatch(setConnectionStatus(true));
    });

    // Start connection
    const startConnection = async () => {
      try {
        await connection.start();
        dispatch(setConnectionStatus(true));
      } catch (error) {
        console.error('SignalR connection failed:', error);
        dispatch(setConnectionStatus(false));
      }
    };

    // Only connect if user is authenticated
    const token = localStorage.getItem('token');
    if (token) {
      startConnection();
    }

    return () => {
      connection.stop();
    };
  }, [dispatch]);

  return <>{children}</>;
};