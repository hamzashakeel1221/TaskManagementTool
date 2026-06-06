import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

interface Profile {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

const ProfilePage: React.FC = () => {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [loading, setLoading] = useState(true);
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    api.get('/users/me')
      .then(res => setProfile(res.data))
      .finally(() => setLoading(false));
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (loading) return <div className="p-8 text-gray-400 text-sm">Loading...</div>;
  if (!profile) return <div className="p-8 text-gray-400 text-sm">Profile not found.</div>;

  return (
    <div className="p-8 max-w-xl">
      <h2 className="text-2xl font-semibold text-gray-800 mb-6">My Profile</h2>

      <div className="bg-white border border-gray-200 rounded-xl p-7">
        <div className="flex items-center gap-4 mb-7 pb-7 border-b border-gray-100">
          <div className="w-16 h-16 rounded-full bg-blue-600 flex items-center justify-center text-2xl font-bold text-white">
            {profile.fullName.charAt(0).toUpperCase()}
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-800">{profile.fullName}</h3>
            <p className="text-sm text-gray-500">{profile.email}</p>
            <span className={`text-xs px-2.5 py-0.5 rounded-full font-medium mt-1 inline-block ${
              profile.role === 'Admin'
                ? 'bg-indigo-100 text-indigo-700'
                : 'bg-gray-100 text-gray-600'
            }`}>
              {profile.role}
            </span>
          </div>
        </div>

        <div className="space-y-4 mb-8">
          {[
            { label: 'Full Name', value: profile.fullName },
            { label: 'Email Address', value: profile.email },
            { label: 'Role', value: profile.role },
            { label: 'User ID', value: profile.id },
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