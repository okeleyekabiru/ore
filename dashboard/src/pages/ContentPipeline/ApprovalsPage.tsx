import { useState } from 'react';
import { useAppSelector } from '../../store/hooks';
import { 
  CheckCircle, 
  XCircle,
  Clock,
  Eye,
  MessageSquare,
  User,
  Calendar,

  Search,
  Download,
  Send
} from 'lucide-react';

// Mock data for approval requests
const mockApprovals = [
  {
    id: '1',
    title: 'Summer Collection Campaign - Instagram Post',
    content: 'Discover our stunning summer collection! Lightweight fabrics, vibrant colors, and sustainable materials come together to create the perfect wardrobe for the season. ☀️✨ #SummerFashion #Sustainable #NewCollection',
    type: 'social_post',
    platform: 'Instagram',
    submittedBy: 'Sarah Johnson',
    submittedAt: '2024-02-15T09:30:00Z',
    status: 'pending',
    priority: 'high',
    scheduledFor: '2024-02-20T14:00:00Z',
    comments: [
      { id: '1', author: 'Mike Chen', content: 'Looks great! Just need to update the hashtags.', createdAt: '2024-02-15T10:15:00Z' }
    ],
    attachments: ['summer-post-image.jpg']
  },
  {
    id: '2',
    title: 'Weekly Newsletter - Product Updates',
    content: 'This week we are excited to share our latest product updates and customer success stories...',
    type: 'newsletter',
    platform: 'Email',
    submittedBy: 'David Lee',
    submittedAt: '2024-02-14T16:20:00Z',
    status: 'approved',
    priority: 'medium',
    scheduledFor: '2024-02-16T09:00:00Z',
    comments: [],
    attachments: []
  },
  {
    id: '3',
    title: 'Product Demo Video Script',
    content: 'Welcome to our latest product demonstration. Today, we will walk you through the key features...',
    type: 'video_script',
    platform: 'YouTube',
    submittedBy: 'Emma Wilson',
    submittedAt: '2024-02-13T11:45:00Z',
    status: 'changes_requested',
    priority: 'low',
    scheduledFor: '2024-02-18T15:30:00Z',
    comments: [
      { id: '2', author: 'Jennifer Adams', content: 'Please add more details about the pricing section.', createdAt: '2024-02-14T09:00:00Z' }
    ],
    attachments: ['script-draft.docx']
  },
];

const statusColors = {
  pending: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
  approved: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  changes_requested: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  rejected: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
};

const priorityColors = {
  high: 'text-red-600 dark:text-red-400',
  medium: 'text-yellow-600 dark:text-yellow-400',
  low: 'text-green-600 dark:text-green-400',
};

export const ApprovalsPage = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [selectedStatus, setSelectedStatus] = useState('all');
  const [selectedPriority, setSelectedPriority] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedApproval, setSelectedApproval] = useState<string | null>(null);
  const [newComment, setNewComment] = useState('');

  // Check if user has team management role (for role-based UI)
  const isEnterpriseUser = user?.role === 'TeamManager' || user?.role === 'Admin';

  if (!isEnterpriseUser) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <CheckCircle className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
            No Approval Required
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mb-6">
            As an individual user, your content is published directly without requiring approval.
          </p>
          <button className="bg-primary-600 hover:bg-primary-700 text-white px-6 py-2 rounded-lg transition-colors">
            Go to Content Generator
          </button>
        </div>
      </div>
    );
  }

  const filteredApprovals = mockApprovals.filter(approval => {
    const matchesSearch = approval.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         approval.content.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = selectedStatus === 'all' || approval.status === selectedStatus;
    const matchesPriority = selectedPriority === 'all' || approval.priority === selectedPriority;
    return matchesSearch && matchesStatus && matchesPriority;
  });

  const handleApprove = (id: string) => {
    console.log('Approving:', id);
    // Implementation for approval action
  };

  const handleReject = (id: string) => {
    console.log('Rejecting:', id);
    // Implementation for rejection action
  };

  const handleRequestChanges = (id: string) => {
    console.log('Requesting changes:', id);
    // Implementation for requesting changes
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="p-6">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            Content Approvals
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Review and approve content before publication
          </p>
        </div>

        {/* Stats Overview */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <Clock className="h-8 w-8 text-yellow-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {mockApprovals.filter(a => a.status === 'pending').length}
                </p>
                <p className="text-gray-600 dark:text-gray-400">Pending</p>
              </div>
            </div>
          </div>
          
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <CheckCircle className="h-8 w-8 text-green-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {mockApprovals.filter(a => a.status === 'approved').length}
                </p>
                <p className="text-gray-600 dark:text-gray-400">Approved</p>
              </div>
            </div>
          </div>
          
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <XCircle className="h-8 w-8 text-red-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {mockApprovals.filter(a => a.status === 'changes_requested').length}
                </p>
                <p className="text-gray-600 dark:text-gray-400">Changes Requested</p>
              </div>
            </div>
          </div>
          
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <Calendar className="h-8 w-8 text-blue-500" />
              <div className="ml-4">
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">24</p>
                <p className="text-gray-600 dark:text-gray-400">This Week</p>
              </div>
            </div>
          </div>
        </div>

        {/* Filters and Search */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow mb-6 p-4">
          <div className="flex flex-col sm:flex-row gap-4">
            {/* Search */}
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search approvals..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              />
            </div>

            {/* Status Filter */}
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white"
            >
              <option value="all">All Status</option>
              <option value="pending">Pending</option>
              <option value="approved">Approved</option>
              <option value="changes_requested">Changes Requested</option>
              <option value="rejected">Rejected</option>
            </select>

            {/* Priority Filter */}
            <select
              value={selectedPriority}
              onChange={(e) => setSelectedPriority(e.target.value)}
              className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white"
            >
              <option value="all">All Priority</option>
              <option value="high">High</option>
              <option value="medium">Medium</option>
              <option value="low">Low</option>
            </select>
          </div>
        </div>

        {/* Approvals List */}
        <div className="space-y-6">
          {filteredApprovals.map((approval) => (
            <div key={approval.id} className="bg-white dark:bg-gray-800 rounded-lg shadow">
              {/* Header */}
              <div className="p-6 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-start justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center space-x-3 mb-2">
                      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                        {approval.title}
                      </h3>
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        statusColors[approval.status as keyof typeof statusColors]
                      }`}>
                        {approval.status.replace('_', ' ').toUpperCase()}
                      </span>
                      <span className={`text-sm font-medium ${
                        priorityColors[approval.priority as keyof typeof priorityColors]
                      }`}>
                        {approval.priority.toUpperCase()} PRIORITY
                      </span>
                    </div>
                    
                    <div className="flex items-center space-x-4 text-sm text-gray-500 dark:text-gray-400">
                      <div className="flex items-center">
                        <User className="h-4 w-4 mr-1" />
                        <span>{approval.submittedBy}</span>
                      </div>
                      <div className="flex items-center">
                        <Calendar className="h-4 w-4 mr-1" />
                        <span>{new Date(approval.submittedAt).toLocaleDateString()}</span>
                      </div>
                      <span className="bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded text-xs">
                        {approval.platform}
                      </span>
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-2 ml-4">
                    <button 
                      onClick={() => setSelectedApproval(selectedApproval === approval.id ? null : approval.id)}
                      className="flex items-center space-x-1 px-3 py-2 text-sm text-gray-600 dark:text-gray-400 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
                    >
                      <Eye className="h-4 w-4" />
                      <span>{selectedApproval === approval.id ? 'Hide' : 'View'}</span>
                    </button>
                  </div>
                </div>
              </div>

              {/* Expanded Content */}
              {selectedApproval === approval.id && (
                <div className="p-6">
                  {/* Content Preview */}
                  <div className="mb-6">
                    <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">Content</h4>
                    <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                      <p className="text-gray-900 dark:text-white whitespace-pre-wrap">
                        {approval.content}
                      </p>
                    </div>
                  </div>

                  {/* Attachments */}
                  {approval.attachments.length > 0 && (
                    <div className="mb-6">
                      <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">Attachments</h4>
                      <div className="flex flex-wrap gap-2">
                        {approval.attachments.map((attachment, index) => (
                          <div key={index} className="flex items-center space-x-2 bg-gray-100 dark:bg-gray-700 px-3 py-2 rounded-lg">
                            <Download className="h-4 w-4 text-gray-500" />
                            <span className="text-sm text-gray-700 dark:text-gray-300">{attachment}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Comments */}
                  <div className="mb-6">
                    <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">Comments</h4>
                    <div className="space-y-3">
                      {approval.comments.map((comment) => (
                        <div key={comment.id} className="bg-gray-50 dark:bg-gray-700 rounded-lg p-3">
                          <div className="flex items-center justify-between mb-2">
                            <span className="font-medium text-gray-900 dark:text-white">{comment.author}</span>
                            <span className="text-xs text-gray-500 dark:text-gray-400">
                              {new Date(comment.createdAt).toLocaleDateString()}
                            </span>
                          </div>
                          <p className="text-gray-700 dark:text-gray-300">{comment.content}</p>
                        </div>
                      ))}
                      
                      {/* Add Comment */}
                      <div className="flex space-x-3">
                        <input
                          type="text"
                          placeholder="Add a comment..."
                          value={newComment}
                          onChange={(e) => setNewComment(e.target.value)}
                          className="flex-1 p-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white"
                        />
                        <button className="px-4 py-3 bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600">
                          <Send className="h-4 w-4" />
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* Action Buttons */}
                  {approval.status === 'pending' && (
                    <div className="flex items-center space-x-3">
                      <button
                        onClick={() => handleApprove(approval.id)}
                        className="flex items-center space-x-2 bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg transition-colors"
                      >
                        <CheckCircle className="h-4 w-4" />
                        <span>Approve</span>
                      </button>
                      <button
                        onClick={() => handleRequestChanges(approval.id)}
                        className="flex items-center space-x-2 bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded-lg transition-colors"
                      >
                        <MessageSquare className="h-4 w-4" />
                        <span>Request Changes</span>
                      </button>
                      <button
                        onClick={() => handleReject(approval.id)}
                        className="flex items-center space-x-2 bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg transition-colors"
                      >
                        <XCircle className="h-4 w-4" />
                        <span>Reject</span>
                      </button>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Empty State */}
        {filteredApprovals.length === 0 && (
          <div className="text-center py-12">
            <CheckCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              No approvals found
            </h3>
            <p className="text-gray-500 dark:text-gray-400">
              {searchTerm || selectedStatus !== 'all' || selectedPriority !== 'all'
                ? 'Try adjusting your search or filters'
                : 'All content has been reviewed'
              }
            </p>
          </div>
        )}
      </div>
    </div>
  );
};