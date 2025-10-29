import { useState } from 'react';
import { 
  Calendar,
  Plus,
  Clock,
  MapPin,
  Users,
  Edit3,
  Trash2,
  ChevronLeft,
  ChevronRight,
  Filter,
  MoreHorizontal
} from 'lucide-react';

// Mock data for scheduled content
const mockScheduledContent = [
  {
    id: '1',
    title: 'Summer Collection Launch',
    platform: 'Instagram',
    type: 'post',
    scheduledAt: '2024-02-15T14:00:00Z',
    status: 'scheduled',
    content: 'Exciting news! Our summer collection is finally here...',
    image: '/api/placeholder/300/200'
  },
  {
    id: '2', 
    title: 'Weekly Newsletter',
    platform: 'Email',
    type: 'newsletter',
    scheduledAt: '2024-02-16T09:00:00Z',
    status: 'scheduled',
    content: 'This week in fashion: trending styles and exclusive offers...'
  },
  {
    id: '3',
    title: 'Product Demo Video',
    platform: 'YouTube',
    type: 'video',
    scheduledAt: '2024-02-17T16:30:00Z',
    status: 'draft',
    content: 'See our latest product in action...'
  },
];

const platformColors = {
  Instagram: 'bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-300',
  Facebook: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
  Twitter: 'bg-sky-100 text-sky-800 dark:bg-sky-900 dark:text-sky-300',
  LinkedIn: 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-300',
  YouTube: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  TikTok: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
  Email: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
};

export const SchedulerPage = () => {
  const [currentDate, setCurrentDate] = useState(new Date());
  const [viewMode, setViewMode] = useState<'month' | 'week' | 'list'>('month');
  const [selectedPlatform, setSelectedPlatform] = useState('all');

  const getDaysInMonth = (date: Date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    const firstDay = new Date(year, month, 1);
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - firstDay.getDay());
    
    const days = [];
    for (let i = 0; i < 42; i++) {
      const day = new Date(startDate);
      day.setDate(startDate.getDate() + i);
      days.push(day);
    }
    return days;
  };

  const getContentForDate = (date: Date) => {
    const dateStr = date.toDateString();
    return mockScheduledContent.filter(content => {
      const contentDate = new Date(content.scheduledAt).toDateString();
      return contentDate === dateStr;
    });
  };

  const navigateMonth = (direction: 'prev' | 'next') => {
    const newDate = new Date(currentDate);
    newDate.setMonth(currentDate.getMonth() + (direction === 'next' ? 1 : -1));
    setCurrentDate(newDate);
  };

  const filteredContent = selectedPlatform === 'all' 
    ? mockScheduledContent 
    : mockScheduledContent.filter(content => content.platform === selectedPlatform);

  const CalendarView = () => {
    const days = getDaysInMonth(currentDate);
    const isCurrentMonth = (date: Date) => date.getMonth() === currentDate.getMonth();
    const isToday = (date: Date) => date.toDateString() === new Date().toDateString();

    return (
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        {/* Calendar Header */}
        <div className="p-4 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
              {currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}
            </h2>
            <div className="flex items-center space-x-2">
              <button 
                onClick={() => navigateMonth('prev')}
                className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg"
              >
                <ChevronLeft className="h-4 w-4 text-gray-600 dark:text-gray-400" />
              </button>
              <button 
                onClick={() => navigateMonth('next')}
                className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg"
              >
                <ChevronRight className="h-4 w-4 text-gray-600 dark:text-gray-400" />
              </button>
            </div>
          </div>
        </div>

        {/* Days of Week */}
        <div className="grid grid-cols-7 border-b border-gray-200 dark:border-gray-700">
          {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map((day) => (
            <div key={day} className="p-3 text-center text-sm font-medium text-gray-500 dark:text-gray-400">
              {day}
            </div>
          ))}
        </div>

        {/* Calendar Grid */}
        <div className="grid grid-cols-7">
          {days.map((day, index) => {
            const dayContent = getContentForDate(day);
            return (
              <div
                key={index}
                className={`min-h-[120px] p-2 border-b border-r border-gray-200 dark:border-gray-700 ${
                  !isCurrentMonth(day) ? 'bg-gray-50 dark:bg-gray-900' : ''
                }`}
              >
                <div className={`text-sm font-medium mb-2 ${
                  !isCurrentMonth(day) 
                    ? 'text-gray-400 dark:text-gray-600'
                    : isToday(day)
                    ? 'text-primary-600 dark:text-primary-400 bg-primary-100 dark:bg-primary-900 rounded-full w-6 h-6 flex items-center justify-center'
                    : 'text-gray-900 dark:text-white'
                }`}>
                  {day.getDate()}
                </div>
                <div className="space-y-1">
                  {dayContent.slice(0, 3).map((content) => (
                    <div
                      key={content.id}
                      className={`text-xs p-1 rounded truncate ${
                        platformColors[content.platform as keyof typeof platformColors] || platformColors.Email
                      }`}
                    >
                      {content.title}
                    </div>
                  ))}
                  {dayContent.length > 3 && (
                    <div className="text-xs text-gray-500 dark:text-gray-400">
                      +{dayContent.length - 3} more
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  const ListView = () => (
    <div className="space-y-4">
      {filteredContent.map((content) => (
        <div key={content.id} className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <div className="flex items-start justify-between">
            <div className="flex-1 min-w-0">
              <div className="flex items-center space-x-3 mb-2">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  {content.title}
                </h3>
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                  platformColors[content.platform as keyof typeof platformColors] || platformColors.Email
                }`}>
                  {content.platform}
                </span>
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                  content.status === 'scheduled' 
                    ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
                    : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300'
                }`}>
                  {content.status}
                </span>
              </div>
              
              <p className="text-gray-600 dark:text-gray-400 mb-4 line-clamp-2">
                {content.content}
              </p>
              
              <div className="flex items-center space-x-4 text-sm text-gray-500 dark:text-gray-400">
                <div className="flex items-center">
                  <Calendar className="h-4 w-4 mr-1" />
                  <span>{new Date(content.scheduledAt).toLocaleDateString()}</span>
                </div>
                <div className="flex items-center">
                  <Clock className="h-4 w-4 mr-1" />
                  <span>{new Date(content.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                </div>
              </div>
            </div>
            
            <div className="flex items-center space-x-2 ml-4">
              <button className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300">
                <Edit3 className="h-4 w-4" />
              </button>
              <button className="p-2 text-gray-400 hover:text-red-600 dark:hover:text-red-400">
                <Trash2 className="h-4 w-4" />
              </button>
              <button className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300">
                <MoreHorizontal className="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      ))}
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="p-6">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
                Content Scheduler
              </h1>
              <p className="text-gray-600 dark:text-gray-400 mt-2">
                Plan and schedule your content across all platforms
              </p>
            </div>
            <button className="flex items-center space-x-2 bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-lg transition-colors">
              <Plus className="h-4 w-4" />
              <span>Schedule Content</span>
            </button>
          </div>
        </div>

        {/* Controls */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow mb-6 p-4">
          <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
            {/* View Mode */}
            <div className="flex items-center space-x-1 bg-gray-100 dark:bg-gray-700 rounded-lg p-1">
              {(['month', 'week', 'list'] as const).map((mode) => (
                <button
                  key={mode}
                  onClick={() => setViewMode(mode)}
                  className={`px-3 py-1 rounded-md text-sm font-medium transition-colors ${
                    viewMode === mode
                      ? 'bg-white dark:bg-gray-600 text-gray-900 dark:text-white shadow-sm'
                      : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
                  }`}
                >
                  {mode.charAt(0).toUpperCase() + mode.slice(1)}
                </button>
              ))}
            </div>

            {/* Filters */}
            <div className="flex items-center space-x-4">
              <select
                value={selectedPlatform}
                onChange={(e) => setSelectedPlatform(e.target.value)}
                className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white"
              >
                <option value="all">All Platforms</option>
                {Object.keys(platformColors).map((platform) => (
                  <option key={platform} value={platform}>{platform}</option>
                ))}
              </select>
              
              <button className="flex items-center space-x-2 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700">
                <Filter className="h-4 w-4" />
                <span>More Filters</span>
              </button>
            </div>
          </div>
        </div>

        {/* Content */}
        {viewMode === 'list' ? <ListView /> : <CalendarView />}

        {/* Stats Summary */}
        <div className="mt-8 grid grid-cols-1 md:grid-cols-4 gap-6">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <Calendar className="h-8 w-8 text-blue-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {mockScheduledContent.filter(c => c.status === 'scheduled').length}
                </p>
                <p className="text-gray-600 dark:text-gray-400">Scheduled</p>
              </div>
            </div>
          </div>
          
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <Clock className="h-8 w-8 text-yellow-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {mockScheduledContent.filter(c => c.status === 'draft').length}
                </p>
                <p className="text-gray-600 dark:text-gray-400">Drafts</p>
              </div>
            </div>
          </div>
          
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <Users className="h-8 w-8 text-green-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">7</p>
                <p className="text-gray-600 dark:text-gray-400">Platforms</p>
              </div>
            </div>
          </div>
          
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <MapPin className="h-8 w-8 text-purple-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">24</p>
                <p className="text-gray-600 dark:text-gray-400">This Week</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};