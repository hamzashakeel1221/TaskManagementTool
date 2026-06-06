import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const ProfilePage: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!user) return <div className="p-8 text-gray-400 text-sm">Profile not found.</div>;

  return (
    <div className="p-8 max-w-xl">
      <h2 className="text-2xl font-semibold text-gray-800 mb-6">My Profile</h2>

      <div className="bg-white border border-gray-200 rounded-xl p-7">
        <div className="flex items-center gap-4 mb-7 pb-7 border-b border-gray-100">
          <div className="w-16 h-16 rounded-full bg-blue-600 flex items-center justify-center text-2xl font-bold text-white">
            {user.fullName.charAt(0).toUpperCase()}
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-800">{user.fullName}</h3>
            <p className="text-sm text-gray-500">{user.email}</p>
            <span className={`text-xs px-2.5 py-0.5 rounded-full font-medium mt-1 inline-block ${
              user.role === 'Admin'
                ? 'bg-indigo-100 text-indigo-700'
                : 'bg-gray-100 text-gray-600'
            }`}>
              {user.role}
            </span>
          </div>
        </div>

        <div className="space-y-4 mb-8">
          {[
            { label: 'Full Name', value: user.fullName },
            { label: 'Email Address', value: user.email },
            { label: 'Role', value: user.role },
            { label: 'User ID', value: user.id },
          ].map(item => (
            <div key={item.label} className="flex justify-between items-center py-2 border-b border-gray-50">
              <span className="text-sm text-gray-500">{item.label}</span>
              <span className="text-sm font-medium text-gray-700">{item.value}</span>
            </div>
          ))}
        </div>

        <button
          onClick={handleLogout}
          className="w-full bg-red-50 hover:bg-red-100 text-red-600 font-medium py-2.5 rounded-lg transition-colors text-sm"
        >
          Logout
        </button>
      </div>
    </div>
  );
};

export default ProfilePage;