import { NavLink, Outlet, useLocation } from 'react-router-dom';
import { Activity, Bell, ChevronDown, CircleHelp, FolderKanban, LayoutDashboard, LogOut, Plus, Search, Settings2, Users } from 'lucide-react';
import { useAuth } from '../auth';

const pageNames: Record<string, string> = {
  '/': 'Overview', '/projects': 'Projects',
};

export function AppShell() {
  const { user, logout } = useAuth();
  const location = useLocation();
  const title = location.pathname.startsWith('/projects/') ? 'Project workspace' : pageNames[location.pathname] ?? 'Workspace';

  return <div className="app-shell">
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark"><Activity size={19} strokeWidth={2.8} /></span><span>taskflow<span className="brand-period">.</span></span></div>
      <div className="workspace-switch"><span className="workspace-avatar">N</span><span className="workspace-copy"><b>Northstar Studio</b><small>Free workspace</small></span><ChevronDown size={15} /></div>
      <div className="nav-section-label">WORKSPACE</div>
      <nav className="main-nav">
        <NavLink to="/" end><LayoutDashboard size={18} /> Overview</NavLink>
        <NavLink to="/projects"><FolderKanban size={18} /> Projects <span className="nav-count">{''}</span></NavLink>
      </nav>
      <div className="nav-section-label nav-label-spaced">MANAGE</div>
      <nav className="main-nav muted-nav">
        <a href="#team" onClick={e => e.preventDefault()}><Users size={18} /> Team</a>
        <a href="#activity" onClick={e => e.preventDefault()}><Activity size={18} /> Activity</a>
        <a href="#settings" onClick={e => e.preventDefault()}><Settings2 size={18} /> Settings</a>
      </nav>
      <div className="sidebar-bottom">
        <div className="upgrade-card"><div className="upgrade-icon"><Plus size={16}/></div><b>Make work flow.</b><p>Bring your team together and move projects forward.</p><button type="button" onClick={() => window.location.assign('/projects')}>Explore projects <span>↗</span></button></div>
        <a className="help-link" href="#help" onClick={e => e.preventDefault()}><CircleHelp size={17}/> Help center</a>
        <div className="profile-row"><div className="avatar">{user?.name?.slice(0, 1).toUpperCase() ?? 'U'}</div><div className="profile-copy"><b>{user?.name ?? 'User'}</b><small>{user?.role ?? 'Member'}</small></div><button className="icon-button" title="Sign out" onClick={logout}><LogOut size={17}/></button></div>
      </div>
    </aside>
    <main className="main-area">
      <header className="topbar"><div className="breadcrumb"><span>Workspace</span><span className="crumb-slash">/</span><b>{title}</b></div><div className="top-actions"><button className="search-trigger"><Search size={16}/><span>Search anything</span><kbd>⌘ K</kbd></button><button className="icon-button notification-button" aria-label="Notifications"><Bell size={18}/><i/></button><div className="top-avatar">{user?.name?.slice(0, 1).toUpperCase() ?? 'U'}</div></div></header>
      <div className="page-content"><Outlet /></div>
    </main>
  </div>;
}
