import React from 'react';
import { useAuth } from '../context/AuthContext';

export const ExpertWorkbenchPage: React.FC = () => {
  const { session, logout } = useAuth();

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 720, margin: '0 auto' }}>
      <h1>Expert Workbench</h1>
      <p>Welcome back, {session?.email ?? 'Expert user'}.</p>
      <p>This page is protected for Expert users only and demonstrates the route guard matrix.</p>
      <button onClick={logout} style={{ padding: '0.85rem 1.25rem', background: '#c72c41', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer' }}>
        Logout
      </button>
    </main>
  );
};
