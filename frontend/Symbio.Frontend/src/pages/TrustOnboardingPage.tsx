import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

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

    apiRequest<ProfileData>('/api/onboarding/profile', { token: session.token })
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

    try
    {
      await apiRequest<{ message: string }>('/api/onboarding/profile', {
        method: 'POST',
        token: session.token,
        body: {
          email: session.email,
          companyName,
          businessIdentifier,
          profileSummary,
        },
      });
    }
    catch
    {
      setIsSubmitting(false);
      setError('Failed to submit onboarding updates.');
      return;
    }

    setIsSubmitting(false);

    setSuccess('Trust onboarding completed successfully.');
    navigate(session.role === 'SME' ? '/sme/dashboard' : '/expert/workbench');
  };

  if (!session) {
    return (
      <main className="symbio-page-main" style={{ textAlign: 'center' }}>
        <h1 className="symbio-page-title">Trust onboarding</h1>
        <p>Please log in before completing your trust profile.</p>
      </main>
    );
  }

  return (
    <main className="symbio-page-main">
      <header className="symbio-role-hero">
        <p className="symbio-role-hero-kicker">Trust and identity</p>
        <h1 className="symbio-page-title symbio-page-title--dark">Trust Onboarding</h1>
        <p className="symbio-role-hero-subtitle">Complete your verified profile so Symbio Hub can match you with the right work and service partners.</p>
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
