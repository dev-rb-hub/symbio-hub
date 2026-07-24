import React from 'react';
import { useAuth } from '../context/AuthContext';

export const UserProfilePage: React.FC = () => {
  const { session } = useAuth();

  if (!session) {
    return null;
  }

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 700, margin: '0 auto' }}>
      <h1>Your Trust Profile</h1>
      <p>Access your registered information and onboarding status on Symbio Hub.</p>
      <dl style={{ display: 'grid', gap: '0.75rem', marginTop: '1.5rem' }}>
        <div><strong>Email:</strong> {session.email}</div>
        <div><strong>Role:</strong> {session.role}</div>
      </dl>
      <p style={{ marginTop: '1.5rem', color: '#555' }}>
        Use the onboarding flow to keep your ABN, company details, and professional summary current for trusted marketplace matching.
      </p>
    </main>
  );
};
