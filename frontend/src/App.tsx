import { Navigate, Route, Routes } from 'react-router-dom';
import type { ReactNode } from 'react';
import { useAuth } from './auth';
import { AppShell } from './components/AppShell';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage, RegisterPage } from './pages/AuthPages';
import { ProjectsPage } from './pages/ProjectsPage';
import { ProjectDetailPage } from './pages/ProjectDetailPage';

function ProtectedLayout() {
  const { token, loading } = useAuth();
  if (loading) return <div className="screen-loading">Loading TaskFlow…</div>;
  return token ? <AppShell/> : <Navigate to="/login" replace/>;
}

function PublicOnly({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  return token ? <Navigate to="/" replace/> : <>{children}</>;
}

export default function App() {
  return <Routes>
    <Route path="/login" element={<PublicOnly><LoginPage/></PublicOnly>}/>
    <Route path="/register" element={<PublicOnly><RegisterPage/></PublicOnly>}/>
    <Route element={<ProtectedLayout/>}>
      <Route path="/" element={<DashboardPage/>}/>
      <Route path="/projects" element={<ProjectsPage/>}/>
      <Route path="/projects/:id" element={<ProjectDetailPage/>}/>
    </Route>
    <Route path="*" element={<Navigate to="/" replace/>}/>
  </Routes>;
}
