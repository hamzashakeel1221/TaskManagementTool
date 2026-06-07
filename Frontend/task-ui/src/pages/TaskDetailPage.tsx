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
  categoryName: string;
  dueDate: string | null;
  ownerName: string;
  ownerId: string;
  assignedToName: string | null;
  createdAt: string;
  updatedAt: string;
}

const statusColors: Record<string, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  InProgress: 'bg-blue-100 text-blue-700',
  Completed: 'bg-green-100 text-green-700',
};

const TaskDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [task, setTask] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);
  const [deleteModal, setDeleteModal] = useState(false);
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    api.get(`/tasks/${id}`)
      .then(res => setTask(res.data))
      .finally(() => setLoading(false));
  }, [id]);

  const isOwner = task?.ownerId === user?.id;

  const handleDeleteConfirm = async () => {
    setDeleting(true);
    try {
      await api.delete(`/tasks/${id}`);
      navigate('/tasks');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete task');
      setDeleting(false);
    } finally {
      setDeleteModal(false);
    }
  };

  if (loading) return <div className="p-8 text-gray-400 text-sm">Loading...</div>;
  if (!task) return <div className="p-8 text-gray-400 text-sm">Task not found.</div>;

  return (
    <div className="p-8 max-w-3xl">
      <button
        onClick={() => navigate('/tasks')}
        className="text-sm text-blue-600 hover:underline mb-5 flex items-center gap-1"
      >
        ← Back to Tasks
      </button>

      <div className="bg-white border border-gray-200 rounded-xl p-7">
        <div className="flex items-start justify-between gap-4 mb-5">
          <div>
            <h2 className="text-xl font-semibold text-gray-800">{task.title}</h2>
            <span className={`text-xs px-2.5 py-0.5 rounded-full font-medium mt-2 inline-block ${statusColors[task.status]}`}>
              {task.status}
            </span>
          </div>
          <div className="flex gap-2 flex-shrink-0">
            <button
              onClick={() => navigate(`/tasks/${id}/edit`)}
              className="text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg transition-colors"
            >
              Edit
            </button>
            {isOwner && (
              <button
                onClick={() => setDeleteModal(true)}
                disabled={deleting}
                className="text-sm bg-red-50 hover:bg-red-100 text-red-600 px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
              >
                {deleting ? 'Deleting...' : 'Delete'}
              </button>
            )}
          </div>
        </div>

        {task.description && (
          <p className="text-gray-600 text-sm leading-relaxed mb-6 pb-6 border-b border-gray-100">
            {task.description}
          </p>
        )}

        <div className="grid grid-cols-2 gap-4">
          {[
            { label: 'Priority', value: task.priority },
            { label: 'Category', value: task.categoryName },
            { label: 'Owner', value: task.ownerName },
            { label: 'Assigned To', value: task.assignedToName || 'Unassigned' },
            { label: 'Due Date', value: task.dueDate ? new Date(task.dueDate + 'Z').toLocaleDateString() : 'No due date' },
{ label: 'Created', value: new Date(task.createdAt + 'Z').toLocaleString() },
{ label: 'Last Updated', value: new Date(task.updatedAt + 'Z').toLocaleString() },
          ].map(item => (
            <div key={item.label} className="bg-gray-50 rounded-lg p-3">
              <p className="text-xs text-gray-400 uppercase tracking-wide mb-1">{item.label}</p>
              <p className="text-sm font-medium text-gray-700">{item.value}</p>
            </div>
          ))}
        </div>

        {!isOwner && (
          <div className="mt-4 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3 text-sm text-amber-700">
            You are not the owner of this task. Only the owner can delete it.
          </div>
        )}
      </div>

      <DeleteConfirmModal
        isOpen={deleteModal}
        taskTitle={task.title}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteModal(false)}
      />
    </div>
  );
};

export default TaskDetailPage;