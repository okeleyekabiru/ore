import { useEffect, useState } from 'react';
import { useAppSelector } from '../../store/hooks';
import { 
  BarChart3, 
  Users, 
  FileText, 
  TrendingUp, 
  Clock, 
  CheckCircle, 
  Calendar,
  Activity
} from 'lucide-react';

interface DashboardStats {
  totalSurveys: number;
  activeContent: number;
  pendingApprovals: number;
  scheduledPosts: number;
  totalUsers: number;
  publishedToday: number;
}

interface RecentActivity {
  id: string;
  type: 'survey' | 'content' | 'approval' | 'publish';
  title: string;
  description: string;
  timestamp: string;
  status: 'success' | 'pending' | 'error';
}

export const OverviewPage = () => {
  const { user } = useAppSelector((state) => state.auth);
  const notificationState = useAppSelector((state) => state.notifications);
  const notifications = (notificationState as any)?.notifications || [];
  const [stats, setStats] = useState<DashboardStats>({
    totalSurveys: 0,
    activeContent: 0,
    pendingApprovals: 0,
    scheduledPosts: 0,
    totalUsers: 0,
    publishedToday: 0,
  });
  const [recentActivity, setRecentActivity] = useState<RecentActivity[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Simulate API call to fetch dashboard data
    const fetchDashboardData = async () => {
      setLoading(true);
      try {
        // Mock data - replace with actual API calls
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        setStats({
          totalSurveys: 12,
          activeContent: 8,
          pendingApprovals: user?.role === 'Individual' ? 0 : 3,
          scheduledPosts: 15,
          totalUsers: user?.role === 'Individual' ? 1 : 25,
          publishedToday: 5,
        });

        setRecentActivity([
          {
            id: '1',
            type: 'content',
            title: 'Blog Post Created',
            description: 'New blog post about sustainability created',
            timestamp: '2 hours ago',
            status: 'success',
          },
          {
            id: '2',
            type: 'approval',
            title: 'Content Approved',
            description: 'Social media post approved by marketing team',
            timestamp: '4 hours ago',
            status: 'success',
          },
          {
            id: '3',
            type: 'publish',
            title: 'Post Published',
            description: 'LinkedIn article published successfully',
            timestamp: '6 hours ago',
            status: 'success',
          },
          {
            id: '4',
            type: 'survey',
            title: 'Survey Updated',
            description: 'Brand guidelines survey updated with new questions',
            timestamp: '1 day ago',
            status: 'success',
          },
        ]);
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, [user?.role]);

  const statCards = [
    {
      title: 'Total Surveys',
      value: stats.totalSurveys,
      icon: FileText,
      color: 'bg-blue-500',
      change: '+2 this month',
    },
    {
      title: 'Active Content',
      value: stats.activeContent,
      icon: Activity,
      color: 'bg-green-500',
      change: '+5 this week',
    },
    ...(user?.role !== 'Individual' ? [{
      title: 'Pending Approvals',
      value: stats.pendingApprovals,
      icon: Clock,
      color: 'bg-yellow-500',
      change: '-1 from yesterday',
    }] : []),
    {
      title: 'Scheduled Posts',
      value: stats.scheduledPosts,
      icon: Calendar,
      color: 'bg-purple-500',
      change: '+3 this week',
    },
    ...(user?.role !== 'Individual' ? [{
      title: 'Team Members',
      value: stats.totalUsers,
      icon: Users,
      color: 'bg-indigo-500',
      change: 'No change',
    }] : []),
    {
      title: 'Published Today',
      value: stats.publishedToday,
      icon: TrendingUp,
      color: 'bg-pink-500',
      change: '+2 from yesterday',
    },
  ];

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'survey':
        return FileText;
      case 'content':
        return Activity;
      case 'approval':
        return CheckCircle;
      case 'publish':
        return TrendingUp;
      default:
        return Activity;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'success':
        return 'text-green-600 bg-green-100 dark:text-green-400 dark:bg-green-900';
      case 'pending':
        return 'text-yellow-600 bg-yellow-100 dark:text-yellow-400 dark:bg-yellow-900';
      case 'error':
        return 'text-red-600 bg-red-100 dark:text-red-400 dark:bg-red-900';
      default:
        return 'text-gray-600 bg-gray-100 dark:text-gray-400 dark:bg-gray-800';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            Welcome back, {user ? `${user.firstName} ${user.lastName}` : 'User'}!
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            Here's what's happening with your content today.
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <BarChart3 className="h-8 w-8 text-primary-600" />
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {statCards.map((stat, index) => {
          const IconComponent = stat.icon;
          return (
            <div
              key={index}
              className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6 hover:shadow-lg transition-shadow"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600 dark:text-gray-400">
                    {stat.title}
                  </p>
                  <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                    {stat.value}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">
                    {stat.change}
                  </p>
                </div>
                <div className={`${stat.color} p-3 rounded-lg`}>
                  <IconComponent className="h-6 w-6 text-white" />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Recent Activity & Notifications */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Activity */}
        <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Recent Activity
          </h2>
          <div className="space-y-4">
            {recentActivity.length === 0 ? (
              <p className="text-gray-500 dark:text-gray-400 text-center py-8">
                No recent activity
              </p>
            ) : (
              recentActivity.map((activity) => {
                const IconComponent = getActivityIcon(activity.type);
                return (
                  <div
                    key={activity.id}
                    className="flex items-start space-x-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                  >
                    <div className="flex-shrink-0">
                      <IconComponent className="h-5 w-5 text-gray-500 dark:text-gray-400" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {activity.title}
                      </p>
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        {activity.description}
                      </p>
                      <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">
                        {activity.timestamp}
                      </p>
                    </div>
                    <div className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(activity.status)}`}>
                      {activity.status}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>

        {/* Quick Actions & Recent Notifications */}
        <div className="space-y-6">
          {/* Quick Actions */}
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Quick Actions
            </h2>
            <div className="space-y-2">
              <button className="w-full text-left p-3 rounded-lg border border-gray-200 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                <div className="flex items-center space-x-3">
                  <Activity className="h-5 w-5 text-primary-600" />
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    Create New Content
                  </span>
                </div>
              </button>
              <button className="w-full text-left p-3 rounded-lg border border-gray-200 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                <div className="flex items-center space-x-3">
                  <Calendar className="h-5 w-5 text-primary-600" />
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    Schedule Post
                  </span>
                </div>
              </button>
              <button className="w-full text-left p-3 rounded-lg border border-gray-200 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                <div className="flex items-center space-x-3">
                  <FileText className="h-5 w-5 text-primary-600" />
                  <span className="text-sm font-medium text-gray-900 dark:text-white">
                    Update Survey
                  </span>
                </div>
              </button>
            </div>
          </div>

          {/* Recent Notifications */}
          <div className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Recent Notifications
            </h2>
            <div className="space-y-2">
              {notifications.slice(0, 3).length === 0 ? (
                <p className="text-gray-500 dark:text-gray-400 text-center py-4">
                  No notifications
                </p>
              ) : (
                notifications.slice(0, 3).map((notification: any) => (
                  <div
                    key={notification.id}
                    className={`p-3 rounded-lg border ${
                      !notification.isRead 
                        ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800' 
                        : 'bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-600'
                    }`}
                  >
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {notification.title}
                    </p>
                    <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                      {notification.message}
                    </p>
                  </div>
                ))
              )}
              {notifications.length > 3 && (
                <button className="text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300">
                  View all notifications
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OverviewPage;
