import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { api } from './api/client';
import type { AuthResponse, User } from './types';

type AuthContextValue = {
  user: User | null; token: string | null; loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => sessionStorage.getItem('taskflow.token'));
  const [user, setUser] = useState<User | null>(() => {
    const value = sessionStorage.getItem('taskflow.user');
    if (!value) return null;
    try { return JSON.parse(value) as User; } catch { sessionStorage.removeItem('taskflow.user'); return null; }
  });
  const [loading] = useState(false);

  const accept = (result: AuthResponse) => {
    sessionStorage.setItem('taskflow.token', result.accessToken);
    sessionStorage.setItem('taskflow.user', JSON.stringify(result.user));
    setToken(result.accessToken);
    setUser(result.user);
  };

  const login = async (email: string, password: string) => accept(await api<AuthResponse>('/auth/login', {
    method: 'POST', body: JSON.stringify({ email, password }),
  }));
  const register = async (name: string, email: string, password: string) => accept(await api<AuthResponse>('/auth/register', {
    method: 'POST', body: JSON.stringify({ name, email, password }),
  }));
  const logout = () => {
    sessionStorage.removeItem('taskflow.token');
    sessionStorage.removeItem('taskflow.user');
    setToken(null); setUser(null);
  };

  const value = useMemo(() => ({ user, token, loading, login, register, logout }), [user, token, loading]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth must be used within AuthProvider');
  return value;
}
