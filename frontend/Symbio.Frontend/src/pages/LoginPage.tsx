import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth, UserRole } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

const getDefaultRouteForRole = (role: UserRole): string => {
  switch (role) {
    case 'SME':
      return '/sme/dashboard';
    case 'Expert':
      return '/expert/dashboard';
    case 'Admin':
      return '/admin/control';
    default:
      return '/';
  }
};

const canRoleAccessPath = (role: UserRole, path: string): boolean => {
  if (!path || path === '/') {
    return true;
  }

  if (path.startsWith('/sme/')) {
    return role === 'SME';
  }

  if (path.startsWith('/expert/')) {
    return role === 'Expert';
  }

  if (path.startsWith('/admin/')) {
    return role === 'Admin';
  }

  if (path === '/talent/discovery' || path === '/project/new' || path === '/billing/control-center') {
    return role === 'SME';
  }

  if (path === '/escrow/onboarding') {
    return role === 'Expert';
  }

  if (path === '/onboarding' || path === '/profile' || path === '/agreement') {
    return role === 'SME' || role === 'Expert' || role === 'Admin';
  }

  return true;
};

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
      let result: { token: string; role?: UserRole };

      try {
        result = await apiRequest<{ token: string; role?: UserRole }>('/api/auth/login', {
          method: 'POST',
          body: { email, password },
        });
      } catch (loginError) {
        if (!(loginError instanceof Error) || !loginError.message.includes('401')) {
          throw loginError;
        }

        result = await apiRequest<{ token: string; role?: UserRole }>('/api/auth/register', {
          method: 'POST',
          body: { email, password, role },
        });
      }

      const authenticatedRole = result.role || role;
      login(result.token, authenticatedRole, email);

      const nextPath = canRoleAccessPath(authenticatedRole, from)
        ? from
        : getDefaultRouteForRole(authenticatedRole);

      navigate(nextPath, { replace: true });
    }
    catch (requestError)
    {
      if (requestError instanceof Error && requestError.message.includes('403')) {
        setError('Admin accounts cannot self-register. Use a seeded admin account for operations access.');
      } else if (requestError instanceof Error && requestError.message.includes('409')) {
        setError('This email is already tied to a different role. Use the correct role for this account or register with a different email.');
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
