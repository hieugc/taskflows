import { useEffect, useState, type FormEvent } from 'react';
import { ArrowUpRight, CalendarDays, FolderKanban, Plus, Search, Users, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import { useAuth } from '../auth';
import { EmptyState } from './DashboardPage';
import type { Project } from '../types';

export function ProjectsPage() {
  const { token, user } = useAuth(); const [projects, setProjects] = useState<Project[]>([]); const [loading, setLoading] = useState(true);
  const [error, setError] = useState(''); const [query, setQuery] = useState(''); const [showCreate, setShowCreate] = useState(false);
  const load = () => api<Project[]>('/projects', {}, token).then(setProjects).catch(e => setError(e instanceof Error ? e.message : 'Unable to load projects')).finally(() => setLoading(false));
  useEffect(() => { void load(); }, [token]);
  const filtered = projects.filter(p => `${p.name} ${p.description}`.toLowerCase().includes(query.toLowerCase()));
  const canCreate = user?.role === 'Admin' || user?.role === 'Manager';
  return <div className="projects-page">
    <div className="page-heading"><div><span className="eyebrow">YOUR WORK, ORGANIZED</span><h1>Projects</h1><p>Keep the big picture clear and the next step closer.</p></div>{canCreate && <button className="primary-button" onClick={() => setShowCreate(true)}><Plus size={17}/> New project</button>}</div>
    <div className="project-toolbar"><div className="project-count"><b>{projects.length}</b> active projects</div><label className="project-search"><Search size={16}/><input aria-label="Search projects" placeholder="Find a project…" value={query} onChange={e => setQuery(e.target.value)}/></label></div>
    {error && <div className="inline-error" role="alert">{error}</div>}
    {loading ? <div className="project-grid">{[1,2,3].map(i => <div className="project-card skeleton-card" key={i}/>)}</div> : filtered.length === 0 ? <EmptyState title={query ? 'No matching projects' : 'No projects yet'} text={query ? 'Try another search term.' : 'Create your first project to start organizing work.'}/> : <div className="project-grid">{filtered.map((project, index) => <ProjectCard project={project} index={index} key={project.id}/>)}</div>}
    {showCreate && <CreateProjectModal token={token} onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); void load(); }}/ >}
  </div>;
}

function ProjectCard({ project, index }: { project: Project; index: number }) {
  const palettes = ['project-purple', 'project-blue', 'project-mint', 'project-amber'];
  const progress = project.taskCount ? Math.round(project.completedTaskCount / project.taskCount * 100) : 0;
  return <Link to={`/projects/${project.id}`} className="project-card"><div className="project-card-top"><div className={`project-symbol ${palettes[index % palettes.length]}`}><FolderKanban size={19}/></div><span className="project-card-arrow"><ArrowUpRight size={17}/></span></div><h3>{project.name}</h3><p className="project-description">{project.description || 'No description added yet.'}</p><div className="project-progress-label"><span>Progress</span><b>{progress}%</b></div><div className="project-progress-track"><span style={{ width: `${progress}%` }}/></div><div className="project-card-meta"><span><Users size={14}/>{project.memberCount} members</span><span><CalendarDays size={14}/>{project.dueDate ? new Date(`${project.dueDate}T00:00:00`).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }) : 'No due date'}</span></div></Link>;
}

function CreateProjectModal({ token, onClose, onCreated }: { token: string | null; onClose: () => void; onCreated: () => void }) {
  const [name, setName] = useState(''); const [description, setDescription] = useState(''); const [startDate, setStartDate] = useState(''); const [dueDate, setDueDate] = useState('');
  const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  const submit = async (e: FormEvent) => { e.preventDefault(); setBusy(true); setError(''); try { await api('/projects', { method: 'POST', body: JSON.stringify({ name, description, startDate: startDate || null, dueDate: dueDate || null }) }, token); onCreated(); } catch (err) { setError(err instanceof Error ? err.message : 'Unable to create project'); } finally { setBusy(false); } };
  return <div className="modal-backdrop" role="presentation" onMouseDown={e => { if (e.target === e.currentTarget) onClose(); }}><section className="modal-card" role="dialog" aria-modal="true" aria-labelledby="create-project-title"><div className="modal-heading"><div><span className="eyebrow">NEW WORKSPACE PROJECT</span><h2 id="create-project-title">Create a project</h2></div><button className="icon-button" onClick={onClose} aria-label="Close"><X size={19}/></button></div><form className="modal-form" onSubmit={submit}><label>Project name<input value={name} onChange={e => setName(e.target.value)} placeholder="e.g. Website redesign" minLength={2} required autoFocus/></label><label>Description<textarea value={description} onChange={e => setDescription(e.target.value)} placeholder="What is this project about?" rows={3}/></label><div className="form-two-col"><label>Start date<input type="date" value={startDate} onChange={e => setStartDate(e.target.value)}/></label><label>Due date<input type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} min={startDate || undefined}/></label></div>{error && <p className="form-error">{error}</p>}<div className="modal-actions"><button type="button" className="secondary-button" onClick={onClose}>Cancel</button><button className="primary-button" disabled={busy}>{busy ? 'Creating…' : 'Create project'} <Plus size={16}/></button></div></form></section></div>;
}
