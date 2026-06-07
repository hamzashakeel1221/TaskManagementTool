import React, { createContext, useContext, useState, useEffect, useMemo } from 'react';

interface AuthUser {
  token: string;
  email: string;
  fullName: string;
  role: string;
  id: string;
}

interface AuthContextType {
  user: AuthUser | null;
  login: (userData: AuthUser) => void;
  logout: () => void;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem('user');
    if (stored) {
      try {
        const parsed: AuthUser = JSON.parse(stored);
        const payload = JSON.parse(atob(parsed.token.split('.')[1]));
        if (payload.exp * 1000 < Date.now()) {
          localStorage.clear();
        } else {
          setUser(parsed);
        }
      } catch {
        localStorage.clear();
      }
    }
  }, []);

  const login = (userData: AuthUser) => {
    localStorage.clear();
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('token', userData.token);
  };

  const logout = () => {
    setUser(null);
    localStorage.clear();
  };

  // ← FIXED: wrap in useMemo so the context value object doesn't change every render
  const value = useMemo<AuthContextType>(
    () => ({ user, login, logout, isAdmin: user?.role === 'Admin' }),
    [user]
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};