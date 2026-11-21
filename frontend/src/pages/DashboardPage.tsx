import { useEffect, useState } from 'react';
import { Activity, ArrowDownRight, ArrowRight, CalendarDays, CheckCircle2, CircleDot, Clock3, FolderKanban, ListTodo, Plus, TrendingUp } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useAuth } from '../auth';
import { api } from '../api/client';
import { StatCard } from '../components/StatCard';
import type { DashboardSummary, WorkTask } from '../types';

const colors: Record<string, string> = { Todo: 'var(--slate)', InProgress: 'var(--blue)', InReview: 'var(--violet)', Done: 'var(--green)', Blocked: 'var(--red)' };
const pretty = (value: string) => value.replace(/([A-Z])/g, ' $1').trim();

export function DashboardPage() {
  const { token, user } = useAuth(); const [summary, setSummary] = useState<DashboardSummary | null>(null); const [error, setError] = useState(''); const [loading, setLoading] = useState(true);
  useEffect(() => { let active = true; api<DashboardSummary>('/dashboard/summary', {}, token).then(data => { if (active) setSummary(data); }).catch(e => { if (active) setError(e instanceof Error ? e.message : 'Unable to load dashboard'); }).finally(() => { if (active) setLoading(false); }); return () => { active = false; }; }, [token]);
  const total = summary?.statusBreakdown.reduce((n, item) => n + item.count, 0) ?? 0;
  return <div className="dashboard-page">
    <div className="page-heading"><div><span className="eyebrow">MONDAY, YOUR WAY</span><h1>Good morning, {user?.name?.split(' ')[0] ?? 'there'} <span className="wave">✦</span></h1><p>Here's what's happening across your workspace today.</p></div><Link to="/projects" className="primary-button"><Plus size={17}/> New project</Link></div>
    {error && <div className="inline-error" role="alert">{error}</div>}
    <section className="stats-grid">
      <StatCard label="Active projects" value={loading ? '—' : summary?.projectCount ?? 0} note="Across your workspace" icon={<FolderKanban size={18}/>} tone="tone-violet"/>
      <StatCard label="Open tasks" value={loading ? '—' : summary?.openTaskCount ?? 0} note="Still in progress" icon={<ListTodo size={18}/>} tone="tone-blue"/>
      <StatCard label="Completed tasks" value={loading ? '—' : summary?.completedTaskCount ?? 0} note="Marked as done" icon={<CheckCircle2 size={18}/>} tone="tone-green"/>
      <StatCard label="Overdue tasks" value={loading ? '—' : summary?.overdueTaskCount ?? 0} note="Need your attention" icon={<Clock3 size={18}/>} tone="tone-orange"/>
    </section>
    <section className="dashboard-grid">
      <article className="panel progress-panel"><div className="panel-heading"><div><h2>Task overview</h2><p>A snapshot of work across your projects</p></div><span className="period-pill"><CalendarDays size={14}/> All time</span></div>
        {loading ? <div className="skeleton-block"/> : total === 0 ? <EmptyState title="No tasks to track yet" text="Create a project and add a few tasks to see progress here."/> : <div className="chart-layout"><div className="donut-wrap"><div className="donut" style={{ background: `conic-gradient(${(summary?.statusBreakdown ?? []).map((x, i, arr) => `${colors[x.name] ?? 'var(--slate)'} ${arr.slice(0, i).reduce((a, y) => a + y.count / total * 100, 0)}% ${arr.slice(0, i + 1).reduce((a, y) => a + y.count / total * 100, 0)}%`).join(', ')})` }}><div className="donut-center"><b>{total}</b><span>Total tasks</span></div></div></div><div className="legend-list">{(summary?.statusBreakdown ?? []).map(item => <div className="legend-row" key={item.name}><span className="legend-dot" style={{ background: colors[item.name] ?? 'var(--slate)' }}/><span>{pretty(item.name)}</span><b>{item.count}</b><small>{Math.round(item.count / total * 100)}%</small></div>)}</div></div>}
      </article>
      <article className="panel priority-panel"><div className="panel-heading"><div><h2>Priority distribution</h2><p>Where the team's focus is</p></div><span className="soft-icon"><TrendingUp size={17}/></span></div>
        {loading ? <div className="skeleton-block"/> : (summary?.priorityBreakdown.length ?? 0) === 0 ? <EmptyState title="No priorities yet" text="Task priorities will appear here."/> : <div className="priority-bars">{['Urgent', 'High', 'Medium', 'Low'].map(name => { const count = summary?.priorityBreakdown.find(x => x.name === name)?.count ?? 0; const max = Math.max(1, ...(summary?.priorityBreakdown.map(x => x.count) ?? [1])); return <div className="priority-bar-row" key={name}><div className="priority-bar-label"><span>{name}</span><b>{count}</b></div><div className="bar-track"><div className={`bar-fill priority-${name.toLowerCase()}`} style={{ width: `${count / max * 100}%` }}/></div></div>; })}</div>}
      </article>
    </section>
    <section className="panel recent-panel"><div className="panel-heading"><div><h2>Recently updated</h2><p>Latest movement across your projects</p></div><Link to="/projects" className="text-link">View all projects <ArrowRight size={15}/></Link></div>
      {loading ? <div className="skeleton-block"/> : (summary?.recentTasks.length ?? 0) === 0 ? <EmptyState title="Your activity starts here" text="When your team updates tasks, you'll see the latest changes here."/> : <div className="table-wrap"><table className="task-table"><thead><tr><th>Task</th><th>Status</th><th>Priority</th><th>Assignee</th><th>Due date</th></tr></thead><tbody>{summary?.recentTasks.map(task => <RecentTaskRow task={task} key={task.id}/>)}</tbody></table></div>}
    </section>
    <div className="dashboard-footnote"><span><CircleDot size={14}/> Workspace health</span><b>All systems looking good</b><span className="health-indicator"/></div>
  </div>;
}

function RecentTaskRow({ task }: { task: WorkTask }) {
  return <tr><td><div className="task-name-cell"><span className="task-mini-icon"><Activity size={14}/></span><b>{task.title}</b></div></td><td><span className={`status-pill status-${task.status.toLowerCase()}`}><i/>{pretty(task.status)}</span></td><td><span className={`priority-label priority-text-${task.priority.toLowerCase()}`}>{task.priority}</span></td><td><div className="assignee-cell"><span className="tiny-avatar">{task.assigneeName?.slice(0,1).toUpperCase() ?? '—'}</span>{task.assigneeName ?? 'Unassigned'}</div></td><td className="date-cell">{task.dueDate ? new Date(`${task.dueDate}T00:00:00`).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }) : '—'}</td></tr>;
}

export function EmptyState({ title, text }: { title: string; text: string }) { return <div className="empty-state"><div className="empty-icon"><ArrowDownRight size={20}/></div><b>{title}</b><p>{text}</p></div>; }
