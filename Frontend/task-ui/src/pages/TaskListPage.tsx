import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import DeleteConfirmModal from '../components/DeleteConfirmModal';
import { useAuth } from '../context/AuthContext';

interface Task {
  id: number;
  title: string;
  status: string;
  priority: string;
  categoryName: string;
  dueDate: string | null;
  assignedToName: string | null;
  ownerName: string;
  ownerId: string;
}

const statusColors: Record<string, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  InProgress: 'bg-blue-100 text-blue-700',
  Completed: 'bg-green-100 text-green-700',
};

const priorityColors: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-600',
  Medium: 'bg-orange-100 text-orange-600',
  High: 'bg-red-100 text-red-600',
};

const TaskListPage: React.FC = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [filter, setFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [deleteModal, setDeleteModal] = useState<{ open: boolean; taskId: number | null; taskTitle: string }>({
    open: false,
    taskId: null,
    taskTitle: '',
  });
  const navigate = useNavigate();
  const { user } = useAuth();

  useEffect(() => {
    api.get('/tasks')
      .then(res => setTasks(res.data))
      .finally(() => setLoading(false));
  }, []);

  const filtered = tasks.filter(t => {
    const matchSearch = t.title.toLowerCase().includes(filter.toLowerCase());
    const matchStatus = statusFilter === '' || t.status === statusFilter;
    return matchSearch && matchStatus;
  });

  const handleDeleteClick = (e: React.MouseEvent, taskId: number, taskTitle: string) => {
    e.preventDefault();
    e.stopPropagation();
    setDeleteModal({ open: true, taskId, taskTitle });
  };

  const handleDeleteConfirm = async () => {
    if (!deleteModal.taskId) return;
    try {
      await api.delete(`/tasks/${deleteModal.taskId}`);
      setTasks(prev => prev.filter(t => t.id !== deleteModal.taskId));
    } catch (err) {
      console.error('Failed to delete task', err);
    } finally {
      setDeleteModal({ open: false, taskId: null, taskTitle: '' });
    }
  };

  const handleDeleteCancel = () => {
    setDeleteModal({ open: false, taskId: null, taskTitle: '' });
  };

  // Extract task list content to avoid nested ternary
  const renderContent = () => {
    if (loading) {
      return <div className="text-gray-400 text-sm">Loading tasks...</div>;
    }
    if (filtered.length === 0) {
      return (
        <div className="text-center py-16 text-gray-400">
          <p className="text-4xl mb-3">📋</p>
          <p className="text-sm">No tasks found</p>
        </div>
      );
    }
    return (
      <div className="space-y-3">
        {filtered.map(task => (
          <Link
            key={task.id}
            to={`/tasks/${task.id}`}
            className="block bg-white border border-gray-200 rounded-xl p-5 hover:shadow-md hover:-translate-y-0.5 transition-all"
          >
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1 min-w-0">
                <p className="font-medium text-gray-800 truncate">{task.title}</p>
                <div className="flex items-center gap-2 mt-2 flex-wrap">
                  <span className={`text-xs px-2.5 py-0.5 rounded-full font-medium ${statusColors[task.status]}`}>
                    {task.status}
                  </span>
                  <span className={`text-xs px-2.5 py-0.5 rounded-full font-medium ${priorityColors[task.priority]}`}>
                    {task.priority}
                  </span>
                  <span className="text-xs text-gray-400">📁 {task.categoryName}</span>
                  {task.dueDate && (
                    <span className="text-xs text-gray-400">
                      📅 {new Date(task.dueDate + 'Z').toLocaleDateString()}
                    </span>
                  )}
                </div>
              </div>
              <div className="flex items-center gap-3 flex-shrink-0">
                <div className="text-right">
                  {task.assignedToName && (
                    <p className="text-xs text-gray-400">👤 {task.assignedToName}</p>
                  )}
                  <p className="text-xs text-gray-300 mt-1">by {task.ownerName}</p>
                </div>
                {user?.id === task.ownerId && (
                  <button
                    onClick={(e) => handleDeleteClick(e, task.id, task.title)}
                    className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                    title="Delete task"
                  >
                    🗑️
                  </button>
                )}
              </div>
            </div>
          </Link>
        ))}
      </div>
    );
  };

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-semibold text-gray-800">Tasks</h2>
        <button
          onClick={() => navigate('/tasks/new')}
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          + New Task
        </button>
      </div>

      <div className="flex gap-3 mb-5 flex-wrap">
        <input
          type="text"
          placeholder="Search tasks..."
          value={filter}
          onChange={e => setFilter(e.target.value)}
          className="flex-1 min-w-48 px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
        </select>
      </div>

      {renderContent()}

      <DeleteConfirmModal
        isOpen={deleteModal.open}
        taskTitle={deleteModal.taskTitle}
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
      />
    </div>
  );
};

export default TaskListPage;