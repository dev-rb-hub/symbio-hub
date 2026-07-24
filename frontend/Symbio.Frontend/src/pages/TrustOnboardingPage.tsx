import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProfileData {
  email: string;
  role: string;
  companyName: string;
  businessIdentifier: string;
  profileSummary: string;
  onboardingCompleted: boolean;
  onboardedAt: string | null;
}

export const TrustOnboardingPage: React.FC = () => {
  const { session, logout } = useAuth();
  const navigate = useNavigate();
  const [profile, setProfile] = useState<ProfileData | null>(null);
  const [companyName, setCompanyName] = useState('');
  const [businessIdentifier, setBusinessIdentifier] = useState('');
  const [profileSummary, setProfileSummary] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!session) return;

    fetch('http://localhost:5001/api/onboarding/profile', {
      headers: {
        Authorization: `Bearer ${session.token}`,
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error('Unable to load profile information.');
        }
        return response.json();
      })
      .then(data => {
        setProfile(data);
        setCompanyName(data.companyName || '');
        setBusinessIdentifier(data.businessIdentifier || '');
        setProfileSummary(data.profileSummary || '');
      })
      .catch(() => setError('Failed to load trust onboarding profile.'));
  }, [session]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setSuccess(null);

    if (!session) {
      setError('You must be signed in to complete onboarding.');
      return;
    }

    setIsSubmitting(true);

    const response = await fetch('http://localhost:5001/api/onboarding/profile', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${session.token}`,
      },
      body: JSON.stringify({
        email: session.email,
        companyName,
        businessIdentifier,
        profileSummary,
      }),
    });

    setIsSubmitting(false);

    if (!response.ok) {
      setError('Failed to submit onboarding updates.');
      return;
    }

    setSuccess('Trust onboarding completed successfully.');
    navigate(session.role === 'SME' ? '/sme/dashboard' : '/expert/workbench');
  };

  if (!session) {
    return (
      <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', textAlign: 'center' }}>
        <h1>Trust onboarding</h1>
        <p>Please log in before completing your trust profile.</p>
      </main>
    );
  }

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 700, margin: '0 auto' }}>
      <header>
        <h1>Trust Onboarding</h1>
        <p>Complete your verified profile so Symbio Hub can match you with the right work and service partners.</p>
      </header>

      {error && <div style={{ color: '#a00', marginTop: '1rem' }}>{error}</div>}
      {success && <div style={{ color: '#0a6', marginTop: '1rem' }}>{success}</div>}

      <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '1rem', marginTop: '1.5rem' }}>
        <label>
          Email
          <input type="email" value={session.email} disabled style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem', background: '#f4f5f7' }} />
        </label>
        <label>
          Business / Company Name
          <input value={companyName} onChange={e => setCompanyName(e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>
        <label>
          Business Identifier (ABN / ACN)
          <input value={businessIdentifier} onChange={e => setBusinessIdentifier(e.target.value)} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>
        <label>
          Professional summary
          <textarea value={profileSummary} onChange={e => setProfileSummary(e.target.value)} required rows={5} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
        </label>
        <button type="submit" disabled={isSubmitting} style={{ padding: '0.85rem 1.25rem', background: '#0072ce', color: '#fff', border: 'none', borderRadius: 8 }}>
          {isSubmitting ? 'Submitting...' : 'Complete onboarding'}
        </button>
      </form>

      {profile?.onboardingCompleted && (
        <section style={{ marginTop: '1.5rem', padding: '1rem', background: '#f3faf7', borderRadius: 12 }}>
          <h2>Existing trust profile</h2>
          <p><strong>Company:</strong> {profile.companyName}</p>
          <p><strong>Business ID:</strong> {profile.businessIdentifier}</p>
          <p><strong>Summary:</strong> {profile.profileSummary}</p>
          <p><strong>Onboarded:</strong> {profile.onboardedAt ?? 'Pending'}</p>
        </section>
      )}

      <button onClick={logout} style={{ marginTop: '1.5rem', padding: '0.85rem 1.25rem', background: '#f1f3f6', color: '#111', border: '1px solid #d6d9dd', borderRadius: 8 }}>
        Log out
      </button>
    </main>
  );
};
