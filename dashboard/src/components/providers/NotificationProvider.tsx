import { useAppSelector } from '../../store/hooks';
import { Toast } from '../common/Toast';

interface NotificationProviderProps {
  children: React.ReactNode;
}

export const NotificationProvider = ({ children }: NotificationProviderProps) => {
  const { notifications } = useAppSelector((state) => state.ui);

  return (
    <>
      {children}
      {notifications.show && (
        <Toast
          message={notifications.message}
          type={notifications.type}
          onClose={() => {
            // Will be handled by the Toast component
          }}
        />
      )}
    </>
  );
};