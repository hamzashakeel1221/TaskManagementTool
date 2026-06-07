import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import DeleteConfirmModal from '../components/DeleteConfirmModal';

interface Task {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
  categoryName: string;
  categoryId: number;
  ownerName: string;
  ownerId: string;
  assignedToName: string | null;
  assignedToId: string | null;
}

const statusColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
};

const priorityColors: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-700',
  Medium: 'bg-orange-100 text-orange-700',
  High: 'bg-red-100 text-red-700',
};

const TaskDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();

  const [task, setTask] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const isAdmin = user?.role === 'Admin';
  const isOwner = task?.ownerId === user?.id;

  // Edit button visible only to the task owner (never to admin-only viewers)
  const canEdit = isOwner;

  // Delete button visible only to the task owner
  const canDelete = isOwner;

  useEffect(() => {
    const fetchTask = async () => {
      try {
        const res = await api.get(`/tasks/${id}`);
        setTask(res.data);
      } catch {
        setError('Task not found or you do not have permission to view it.');
      } finally {
        setLoading(false);
      }
    };
    fetchTask();
  }, [id]);

  const handleDelete = async () => {
    try {
      await api.delete(`/tasks/${id}`);
      navigate('/tasks');
    } catch {
      setError('Failed to delete task.');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-violet-600" />
      </div>
    );
  }

  if (error || !task) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-500 text-lg mb-4">{error || 'Task not found.'}</p>
          <button
            onClick={() => navigate('/tasks')}
            className="px-4 py-2 bg-violet-600 text-white rounded-lg hover:bg-violet-700"
          >
            Back to Tasks
          </button>
        </div>
      </div>
    );
  }

  const details = [
    { label: 'Status', value: task.status, badge: statusColors[task.status] },
    { label: 'Priority', value: task.priority, badge: priorityColors[task.priority] },
    { label: 'Category', value: task.categoryName },
    { label: 'Due Date', value: task.dueDate ? new Date(task.dueDate + 'Z').toLocaleDateString() : 'No due date' },
    { label: 'Created', value: new Date(task.createdAt + 'Z').toLocaleString() },
    { label: 'Last Updated', value: new Date(task.updatedAt + 'Z').toLocaleString() },
    { label: 'Owner', value: task.ownerName },
    { label: 'Assigned To', value: task.assignedToName || 'Unassigned' },
  ];

  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-3xl mx-auto">

        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <button
            onClick={() => navigate('/tasks')}
            className="flex items-center gap-2 text-gray-500 hover:text-gray-700 transition-colors"
          >
            ← Back to Tasks
          </button>

          {/* Action buttons — only shown to task owner */}
          <div className="flex gap-3">
            {canEdit && (
              <button
                onClick={() => navigate(`/tasks/${task.id}/edit`)}
                className="px-4 py-2 bg-violet-600 text-white rounded-lg hover:bg-violet-700 transition-colors text-sm font-medium"
              >
                ✏️ Edit Task
              </button>
            )}
            {canDelete && (
              <button
                onClick={() => setShowDeleteModal(true)}
                className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors text-sm font-medium"
              >
                🗑️ Delete
              </button>
            )}
          </div>
        </div>

        {/* Admin view notice — shown when admin is viewing someone else's task */}
        {isAdmin && !isOwner && (
          <div className="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg text-blue-700 text-sm">
            👁️ <strong>Admin view</strong> — You can view this task but cannot edit or delete it as you are not the owner.
          </div>
        )}

        {/* Task Card */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
          <h1 className="text-2xl font-bold text-gray-900 mb-2">{task.title}</h1>
          <p className="text-gray-500 mb-6 leading-relaxed">{task.description || 'No description provided.'}</p>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {details.map(({ label, value, badge }) => (
              <div key={label} className="bg-gray-50 rounded-xl p-4">
                <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-1">{label}</p>
                {badge ? (
                  <span className={`inline-block px-2 py-0.5 rounded-full text-sm font-medium ${badge}`}>
                    {value}
                  </span>
                ) : (
                  <p className="text-sm font-medium text-gray-800">{value}</p>
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Delete Confirm Modal */}
      <DeleteConfirmModal
        isOpen={showDeleteModal}
        taskTitle={task.title}
        onConfirm={handleDelete}
        onCancel={() => setShowDeleteModal(false)}
      />
    </div>
  );
};

export default TaskDetailPage;