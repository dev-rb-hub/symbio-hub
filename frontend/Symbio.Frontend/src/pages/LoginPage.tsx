import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth, UserRole } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('password123');
  const [role, setRole] = useState<UserRole>('SME');
  const [error, setError] = useState<string | null>(null);
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location })?.from?.pathname || '/';

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    try
    {
      const result = await apiRequest<{ token: string; role?: UserRole }>('/api/auth/register', {
        method: 'POST',
        body: { email, password, role },
      });

      login(result.token, result.role || role, email);
      navigate(from, { replace: true });
    }
    catch
    {
      setError('Unable to sign in or register.');
      return;
    }
  };

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 600, margin: '0 auto' }}>
      <h1>Access Symbio Hub</h1>
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
            <option value="Admin">Admin</option>
          </select>
        </label>
        {error && <div style={{ color: '#a00' }}>{error}</div>}
        <button type="submit" style={{ padding: '0.85rem 1.25rem', background: '#0072ce', color: '#fff', border: 'none', borderRadius: 8 }}>Continue</button>
      </form>
    </main>
  );
};
