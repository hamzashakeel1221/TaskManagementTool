import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { CheckCircle, Clock, AlertCircle, ListTodo, Plus, ArrowRight, TrendingUp } from 'lucide-react'
import api from '../api/axios'
import { useAuth } from '../context/AuthContext'

interface Stats { total: number; pending: number; inProgress: number; completed: number }

const DashboardPage: React.FC = () => {
  const [stats, setStats] = useState<Stats | null>(null)
  const [recentTasks, setRecentTasks] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const { user } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    Promise.all([
      api.get('/dashboard'),
      api.get('/tasks')
    ]).then(([dashRes, tasksRes]) => {
      setStats(dashRes.data)
      setRecentTasks(tasksRes.data.slice(0, 5))
    }).finally(() => setLoading(false))
  }, [])

  const statCards = [
    { label: 'Total Tasks', value: stats?.total ?? 0, icon: ListTodo, color: 'text-blue-600', bg: 'bg-blue-50', border: 'border-blue-100' },
    { label: 'Pending', value: stats?.pending ?? 0, icon: AlertCircle, color: 'text-amber-600', bg: 'bg-amber-50', border: 'border-amber-100' },
    { label: 'In Progress', value: stats?.inProgress ?? 0, icon: Clock, color: 'text-indigo-600', bg: 'bg-indigo-50', border: 'border-indigo-100' },
    { label: 'Completed', value: stats?.completed ?? 0, icon: CheckCircle, color: 'text-green-600', bg: 'bg-green-50', border: 'border-green-100' },
  ]

  const statusBadge: Record<string, string> = {
    Pending: 'bg-amber-100 text-amber-700',
    InProgress: 'bg-blue-100 text-blue-700',
    Completed: 'bg-green-100 text-green-700',
  }

  const priorityBadge: Record<string, string> = {
    Low: 'bg-slate-100 text-slate-600',
    Medium: 'bg-orange-100 text-orange-600',
    High: 'bg-red-100 text-red-600',
  }

  const completionRate = stats && stats.total > 0
    ? Math.round((stats.completed / stats.total) * 100)
    : 0

  return (
    <div className="p-6 space-y-6">

      {/* Welcome banner */}
      <div className="bg-gradient-to-r from-blue-600 to-indigo-600 rounded-2xl p-6 flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-white mb-1">
            Good {new Date().getHours() < 12 ? 'morning' : new Date().getHours() < 17 ? 'afternoon' : 'evening'}, {user?.fullName?.split(' ')[0]} 👋
          </h2>
          <p className="text-blue-100 text-sm">
            {user?.role === 'Admin'
              ? 'You have full access to all tasks and team management.'
              : `You have ${stats?.pending ?? 0} pending tasks waiting for you.`}
          </p>
        </div>
        <button
          onClick={() => navigate('/tasks/new')}
          className="bg-white text-blue-600 font-semibold text-sm px-4 py-2.5 rounded-xl hover:bg-blue-50 transition-colors flex items-center gap-2 flex-shrink-0"
        >
          <Plus size={16} /> New Task
        </button>
      </div>

      {/* Stat cards */}
      {loading ? (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          {[1,2,3,4].map(i => (
            <div key={i} className="bg-white rounded-2xl p-5 border border-slate-100 animate-pulse h-28" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          {statCards.map(({ label, value, icon: Icon, color, bg, border }) => (
            <div key={label} className={`bg-white rounded-2xl p-5 border ${border} hover:shadow-md transition-shadow`}>
              <div className="flex items-center justify-between mb-3">
                <div className={`w-10 h-10 ${bg} rounded-xl flex items-center justify-center`}>
                  <Icon size={20} className={color} />
                </div>
                <TrendingUp size={14} className="text-slate-300" />
              </div>
              <p className="text-3xl font-bold text-slate-800 mb-0.5">{value}</p>
              <p className="text-xs text-slate-500 font-medium">{label}</p>
            </div>
          ))}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">

        {/* Recent tasks */}
        <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-100 overflow-hidden">
          <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
            <h3 className="font-semibold text-slate-800 text-sm">Recent Tasks</h3>
            <button
              onClick={() => navigate('/tasks')}
              className="text-xs text-blue-600 hover:underline flex items-center gap-1"
            >
              View all <ArrowRight size={12} />
            </button>
          </div>
          <div className="divide-y divide-slate-50">
            {recentTasks.length === 0 ? (
              <div className="px-6 py-10 text-center text-slate-400 text-sm">No tasks yet</div>
            ) : recentTasks.map(task => (
              <div
                key={task.id}
                onClick={() => navigate(`/tasks/${task.id}`)}
                className="flex items-center gap-4 px-6 py-3.5 hover:bg-slate-50 cursor-pointer transition-colors"
              >
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-slate-700 truncate">{task.title}</p>
                  <p className="text-xs text-slate-400 mt-0.5">{task.categoryName}</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${priorityBadge[task.priority]}`}>
                    {task.priority}
                  </span>
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusBadge[task.status]}`}>
                    {task.status}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Progress panel */}
        <div className="bg-white rounded-2xl border border-slate-100 p-6">
          <h3 className="font-semibold text-slate-800 text-sm mb-5">Progress Overview</h3>

          <div className="flex items-center justify-center mb-6">
            <div className="relative w-28 h-28">
              <svg className="w-28 h-28 -rotate-90" viewBox="0 0 100 100">
                <circle cx="50" cy="50" r="40" fill="none" stroke="#f1f5f9" strokeWidth="10" />
                <circle
                  cx="50" cy="50" r="40" fill="none"
                  stroke="#2563eb" strokeWidth="10"
                  strokeDasharray={`${completionRate * 2.51} 251`}
                  strokeLinecap="round"
                />
              </svg>
              <div className="absolute inset-0 flex flex-col items-center justify-center">
                <span className="text-2xl font-bold text-slate-800">{completionRate}%</span>
                <span className="text-xs text-slate-400">Done</span>
              </div>
            </div>
          </div>

          <div className="space-y-3">
            {[
              { label: 'Completed', value: stats?.completed ?? 0, color: 'bg-green-500' },
              { label: 'In Progress', value: stats?.inProgress ?? 0, color: 'bg-blue-500' },
              { label: 'Pending', value: stats?.pending ?? 0, color: 'bg-amber-500' },
            ].map(item => (
              <div key={item.label} className="flex items-center gap-3">
                <div className={`w-2.5 h-2.5 rounded-full ${item.color} flex-shrink-0`}></div>
                <span className="text-xs text-slate-500 flex-1">{item.label}</span>
                <span className="text-xs font-semibold text-slate-700">{item.value}</span>
              </div>
            ))}
          </div>

          <button
            onClick={() => navigate('/tasks/new')}
            className="mt-6 w-full bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium py-2.5 rounded-xl transition-colors flex items-center justify-center gap-2"
          >
            <Plus size={15} /> Create Task
          </button>
        </div>
      </div>
    </div>
  )
}

export default DashboardPage