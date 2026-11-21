import type { ReactNode } from 'react';
export function StatCard({ label, value, note, icon, tone }: { label: string; value: number | string; note: string; icon: ReactNode; tone: string }) {
  return <article className="stat-card"><div className="stat-top"><span>{label}</span><span className={`stat-icon ${tone}`}>{icon}</span></div><div className="stat-value">{value}</div><div className="stat-note">{note}</div></article>;
}
