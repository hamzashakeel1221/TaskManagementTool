import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

interface Category {
  id: number;
  name: string;
}

interface UserOption {
  id: string;
  fullName: string;
}

interface TaskForm {
  title: string;
  description: string;
  priority: string;
  status: string;
  categoryId: string;
  dueDate: string;
  assignedToId: string;
}

const TaskFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();

  const isEdit = Boolean(id);
  const isAdmin = user?.role === 'Admin';

  const [form, setForm] = useState<TaskForm>({
    title: '',
    description: '',
    priority: 'Medium',
    status: 'Pending',
    categoryId: '',
    dueDate: '',
    assignedToId: '',
  });

  const [categories, setCategories] = useState<Category[]>([]);
  const [users, setUsers] = useState<UserOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [taskOwnerId, setTaskOwnerId] = useState<string | null>(null);

  const isOwner = !isEdit || taskOwnerId === user?.id;

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [catRes, usersRes] = await Promise.all([
          api.get('/categories'),
          isAdmin ? api.get('/users') : Promise.resolve({ data: [] }),
        ]);
        setCategories(catRes.data);
        setUsers(usersRes.data);
      } catch {
        // categories or users failed to load
      }
    };
    fetchData();
  }, [isAdmin]);

  useEffect(() => {
    if (!isEdit) return;

    const fetchTask = async () => {
      try {
        const res = await api.get(`/tasks/${id}`);
        const task = res.data;
        setTaskOwnerId(task.ownerId);

        if (isAdmin && task.ownerId !== user?.id) {
          navigate(`/tasks/${id}`, { replace: true });
          return;
        }

        setForm({
          title: task.title || '',
          description: task.description || '',
          priority: task.priority || 'Medium',
          status: task.status || 'Pending',
          categoryId: task.categoryId?.toString() || '',
          dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
          assignedToId: task.assignedToId || '',
        });
      } catch {
        setError('Failed to load task.');
      }
    };
    fetchTask();
  }, [id, isEdit, isAdmin, user?.id, navigate]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      if (isEdit) {
        const payload = {
          title: form.title,
          description: form.description,
          priority: form.priority,
          status: form.status,
          categoryId: Number(form.categoryId),
          dueDate: form.dueDate || null,
          assignedToId: form.assignedToId || null,
        };
        await api.put(`/tasks/${id}`, payload);
      } else {
        const assignedToId = isAdmin ? form.assignedToId || null : null;
        const payload = {
          title: form.title,
          description: form.description,
          priority: form.priority,
          categoryId: Number(form.categoryId),
          dueDate: form.dueDate || null,
          assignedToId,
        };
        await api.post('/tasks', payload);
      }

      navigate('/tasks');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save task.');
    } finally {
      setLoading(false);
    }
  };

  // ✅ FIX: Nested ternary extracted into independent statement (L141)
  let submitLabel: string;
  if (loading) {
    submitLabel = 'Saving...';
  } else if (isEdit) {
    submitLabel = 'Update Task';
  } else {
    submitLabel = 'Create Task';
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-2xl mx-auto">

        <div className="mb-6">
          <button
            onClick={() => navigate('/tasks')}
            className="text-gray-500 hover:text-gray-700 text-sm mb-4 flex items-center gap-1"
          >
            ← Back to Tasks
          </button>
          <h1 className="text-2xl font-bold text-gray-900">
            {isEdit ? 'Edit Task' : 'Create New Task'}
          </h1>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-600 text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">

          {/* Title */}
          <div>
            <label htmlFor="task-title" className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
            <input
              id="task-title"
              name="title"
              value={form.title}
              onChange={handleChange}
              required
              placeholder="Enter task title"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
            />
          </div>

          {/* Description */}
          <div>
            <label htmlFor="task-description" className="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              id="task-description"
              name="description"
              value={form.description}
              onChange={handleChange}
              rows={3}
              placeholder="Enter task description"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500 resize-none"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

            {/* Priority */}
            <div>
              <label htmlFor="task-priority" className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select
                id="task-priority"
                name="priority"
                value={form.priority}
                onChange={handleChange}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </div>

            {/* Status — shown in edit mode only */}
            {isEdit && (
              <div>
                <label htmlFor="task-status" className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                <select
                  id="task-status"
                  name="status"
                  value={form.status}
                  onChange={handleChange}
                  className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                >
                  <option value="Pending">Pending</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Completed">Completed</option>
                </select>
              </div>
            )}

            {/* Category */}
            <div>
              <label htmlFor="task-category" className="block text-sm font-medium text-gray-700 mb-1">Category *</label>
              <select
                id="task-category"
                name="categoryId"
                value={form.categoryId}
                onChange={handleChange}
                required
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
              >
                <option value="">Select category</option>
                {categories.map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>

            {/* Due Date */}
            <div>
              <label htmlFor="task-duedate" className="block text-sm font-medium text-gray-700 mb-1">Due Date</label>
              <input
                id="task-duedate"
                type="date"
                name="dueDate"
                value={form.dueDate}
                onChange={handleChange}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
              />
            </div>
          </div>

          {/* Assign To — admin only */}
          {isAdmin && isOwner && (
            <div>
              <label htmlFor="task-assignedto" className="block text-sm font-medium text-gray-700 mb-1">
                Assign To <span className="text-xs text-violet-500 font-normal">(Admin only)</span>
              </label>
              <select
                id="task-assignedto"
                name="assignedToId"
                value={form.assignedToId}
                onChange={handleChange}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
              >
                <option value="">Unassigned</option>
                {users.map(u => (
                  <option key={u.id} value={u.id}>{u.fullName}</option>
                ))}
              </select>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="submit"
              disabled={loading}
              className="flex-1 bg-violet-600 text-white py-2 rounded-lg font-medium hover:bg-violet-700 transition-colors disabled:opacity-50 text-sm"
            >
              {submitLabel}
            </button>
            <button
              type="button"
              onClick={() => navigate('/tasks')}
              className="px-4 py-2 border border-gray-200 text-gray-600 rounded-lg hover:bg-gray-50 transition-colors text-sm"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TaskFormPage;