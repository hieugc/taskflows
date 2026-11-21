import { useState, type FormEvent, type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Activity, ArrowRight, Eye, EyeOff } from 'lucide-react';
import { useAuth } from '../auth';

export function LoginPage() {
  const { login } = useAuth(); const navigate = useNavigate();
  const [email, setEmail] = useState(''); const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false); const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent) => { event.preventDefault(); setError(''); setBusy(true); try { await login(email, password); navigate('/'); } catch (e) { setError(e instanceof Error ? e.message : 'Unable to sign in'); } finally { setBusy(false); } };
  return <AuthLayout eyebrow="WELCOME BACK" title="Good to see you again." subtitle="Sign in to pick up where your team left off."><form className="auth-form" onSubmit={submit}>
    <label>Email address<input type="email" autoComplete="email" placeholder="you@company.com" value={email} onChange={e => setEmail(e.target.value)} required /></label>
    <div className="auth-password-field"><label htmlFor="login-password">Password</label><div className="password-wrap"><input id="login-password" type={showPassword ? 'text' : 'password'} autoComplete="current-password" placeholder="Enter your password" value={password} onChange={e => setPassword(e.target.value)} required /><button type="button" className="password-toggle" onClick={() => setShowPassword(!showPassword)} aria-label={showPassword ? 'Hide password' : 'Show password'}>{showPassword ? <EyeOff size={17}/> : <Eye size={17}/>}</button></div></div>
    {error && <p className="form-error" role="alert">{error}</p>}
    <button className="primary-button auth-submit" disabled={busy}>{busy ? 'Signing in…' : 'Sign in'} <ArrowRight size={17}/></button>
    <p className="auth-switch">New to TaskFlow? <Link to="/register">Create an account</Link></p>
  </form></AuthLayout>;
}

export function RegisterPage() {
  const { register } = useAuth(); const navigate = useNavigate();
  const [name, setName] = useState(''); const [email, setEmail] = useState(''); const [password, setPassword] = useState('');
  const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent) => { event.preventDefault(); setError(''); setBusy(true); try { await register(name, email, password); navigate('/'); } catch (e) { setError(e instanceof Error ? e.message : 'Unable to create account'); } finally { setBusy(false); } };
  return <AuthLayout eyebrow="GET STARTED" title="Make room for great work." subtitle="Create your account and bring your projects into focus."><form className="auth-form" onSubmit={submit}>
    <label>Full name<input autoComplete="name" placeholder="Your name" value={name} onChange={e => setName(e.target.value)} minLength={2} required /></label>
    <label>Work email<input type="email" autoComplete="email" placeholder="you@company.com" value={email} onChange={e => setEmail(e.target.value)} required /></label>
    <label>Password<input type="password" autoComplete="new-password" placeholder="At least 10 characters" value={password} onChange={e => setPassword(e.target.value)} minLength={10} required /></label>
    {error && <p className="form-error" role="alert">{error}</p>}
    <button className="primary-button auth-submit" disabled={busy}>{busy ? 'Creating account…' : 'Create account'} <ArrowRight size={17}/></button>
    <p className="auth-switch">Already have an account? <Link to="/login">Sign in</Link></p>
  </form></AuthLayout>;
}

function AuthLayout({ eyebrow, title, subtitle, children }: { eyebrow: string; title: string; subtitle: string; children: ReactNode }) {
  return <div className="auth-layout"><div className="auth-visual"><div className="auth-visual-brand"><span className="brand-mark"><Activity size={19}/></span> taskflow<span className="brand-period">.</span></div><div className="visual-copy"><span className="eyebrow">YOUR TEAM, IN SYNC</span><h2>Good work happens<br/>when work <em>flows.</em></h2><p>One clear place for your projects, people, and progress.</p></div><div className="visual-orbit orbit-one"/><div className="visual-orbit orbit-two"/><div className="visual-footer">Thoughtful work. Meaningful progress.</div></div><div className="auth-panel"><div className="auth-mobile-brand"><span className="brand-mark"><Activity size={19}/></span> taskflow<span className="brand-period">.</span></div><div className="auth-content"><span className="eyebrow">{eyebrow}</span><h1>{title}</h1><p className="auth-subtitle">{subtitle}</p>{children}</div><div className="auth-legal">© 2026 TaskFlow <span>·</span> Built for teams that care about the details.</div></div></div>;
}
