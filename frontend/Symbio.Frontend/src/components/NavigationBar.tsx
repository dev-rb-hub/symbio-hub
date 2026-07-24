import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const NavigationBar: React.FC = () => {
  const { session, logout } = useAuth();

  return (
    <nav style={{ background: '#fff', borderBottom: '1px solid #e1e5ea', padding: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
      <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
        <Link to="/" style={{ color: '#0072ce', fontWeight: 700, textDecoration: 'none' }}>Symbio Hub</Link>
        <Link to="/jobs" style={{ color: '#333', textDecoration: 'none' }}>Public jobs</Link>
        <Link to="/marketplace" style={{ color: '#333', textDecoration: 'none' }}>Marketplace</Link>
        {session && session.role === 'SME' && (
          <Link to="/project/new" style={{ color: '#333', textDecoration: 'none' }}>Post a Project</Link>
        )}
        {session && <Link to="/onboarding" style={{ color: '#333', textDecoration: 'none' }}>Trust onboarding</Link>}
        {session && <Link to="/profile" style={{ color: '#333', textDecoration: 'none' }}>Profile</Link>}
      </div>
      <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
        {session ? (
          <>
            <span style={{ color: '#555' }}>{session.email}</span>
            <button onClick={logout} style={{ padding: '0.5rem 0.85rem', background: '#f1f3f6', border: '1px solid #d6d9dd', borderRadius: 8, cursor: 'pointer' }}>
              Log out
            </button>
          </>
        ) : (
          <Link to="/login" style={{ color: '#0072ce', textDecoration: 'none' }}>Log in</Link>
        )}
      </div>
    </nav>
  );
};
