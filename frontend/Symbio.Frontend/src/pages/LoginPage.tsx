import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth, UserRole } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('password123');
  const [role, setRole] = useState<UserRole>('SME');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location })?.from?.pathname || '/';

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try
    {
      const result = await apiRequest<{ token: string; role?: UserRole }>('/api/auth/register', {
        method: 'POST',
        body: { email, password, role },
      });

      login(result.token, result.role || role, email);
      navigate(from, { replace: true });
    }
    catch (requestError)
    {
      if (requestError instanceof Error && requestError.message.includes('403')) {
        setError('Admin accounts cannot self-register. Use a seeded admin account for operations access.');
      } else {
        setError('Unable to sign in or register.');
      }

      setIsSubmitting(false);
      return;
    }

    setIsSubmitting(false);
  };

  return (
    <main className="symbio-page-main">
      <h1 className="symbio-page-title">Access Symbio Hub</h1>
      <p>Preserves your intended destination if you clicked through while logged out.</p>
      <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '1rem', marginTop: '1.5rem' }}>
        <label>
          Email
          <input value={email} onChange={e => setEmail(e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={e => setPassword(e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>
        <label>
          Role
          <select value={role} onChange={e => setRole(e.target.value as UserRole)} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }}>
            <option value="SME">SME</option>
            <option value="Expert">Expert</option>
          </select>
        </label>
        <p style={{ margin: 0, color: '#555', fontSize: '0.95rem' }}>
          Admin operations users are seeded by the platform and do not self-register via this form.
        </p>
        {error && <div style={{ color: '#a00' }}>{error}</div>}
        <button type="submit" disabled={isSubmitting} style={{ padding: '0.85rem 1.25rem', background: '#0072ce', color: '#fff', border: 'none', borderRadius: 8 }}>
          {isSubmitting ? 'Signing in...' : 'Continue'}
        </button>
      </form>
    </main>
  );
};
