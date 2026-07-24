import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';

interface EscrowStatus {
  expertEmail: string;
  status: string;
  providerAccountId: string;
  onboardingUrl: string;
  lastSyncedAtUtc: string | null;
  onboardedAtUtc: string | null;
}

export const EscrowOnboardingPage: React.FC = () => {
  const { session } = useAuth();
  const [status, setStatus] = useState<EscrowStatus | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isWorking, setIsWorking] = useState(false);

  const loadStatus = async () => {
    if (!session) {
      return;
    }

    const response = await fetch('http://localhost:5001/api/payments/onboarding/status', {
      headers: {
        Authorization: `Bearer ${session.token}`,
      },
    });

    if (!response.ok) {
      setError('Failed to load escrow onboarding status.');
      return;
    }

    const data = await response.json();
    setStatus(data);
    setError(null);
  };

  useEffect(() => {
    void loadStatus();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session]);

  const runAction = async (path: 'start' | 'refresh' | 'simulate-complete') => {
    if (!session) {
      return;
    }

    setIsWorking(true);
    const response = await fetch(`http://localhost:5001/api/payments/onboarding/${path}`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${session.token}`,
      },
    });
    setIsWorking(false);

    if (!response.ok) {
      setError('Escrow onboarding action failed.');
      return;
    }

    const data = await response.json();
    setStatus(data);
    setError(null);
  };

  if (!session) {
    return null;
  }

  const currentStatus = status?.status ?? 'NotStarted';
  const isVerified = currentStatus.toLowerCase() === 'verified';

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 900, margin: '0 auto' }}>
      <header>
        <p style={{ color: '#0072ce', fontWeight: 700, marginBottom: 0 }}>Expert payments</p>
        <h1 style={{ marginTop: '0.35rem' }}>Escrow Onboarding</h1>
        <p style={{ maxWidth: 720, lineHeight: 1.7, color: '#444' }}>
          Connect your expert profile to the Pinch Glassbox onboarding flow so milestone-based escrow settlement can be enabled.
        </p>
      </header>

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      <section style={{ marginTop: '1.5rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))' }}>
        <article style={{ padding: '1.2rem', border: '1px solid #e2e6ed', borderRadius: 16, background: '#fff' }}>
          <div style={{ color: '#555' }}>Current status</div>
          <strong style={{ fontSize: '1.1rem', color: isVerified ? '#0a6' : '#333' }}>{currentStatus}</strong>
        </article>
        <article style={{ padding: '1.2rem', border: '1px solid #e2e6ed', borderRadius: 16, background: '#fff' }}>
          <div style={{ color: '#555' }}>Provider account</div>
          <strong style={{ fontSize: '0.95rem' }}>{status?.providerAccountId || 'Not created'}</strong>
        </article>
      </section>

      <section style={{ marginTop: '1.25rem', padding: '1.2rem', border: '1px solid #e2e6ed', borderRadius: 16, background: '#fff' }}>
        <h2 style={{ marginTop: 0 }}>Onboarding actions</h2>
        <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
          <button type="button" disabled={isWorking} onClick={() => void runAction('start')} style={{ padding: '0.8rem 1.1rem', border: 'none', borderRadius: 8, background: '#0072ce', color: '#fff', cursor: 'pointer' }}>
            Start onboarding
          </button>
          <button type="button" disabled={isWorking} onClick={() => void runAction('refresh')} style={{ padding: '0.8rem 1.1rem', border: '1px solid #d6d9dd', borderRadius: 8, background: '#f1f3f6', color: '#111', cursor: 'pointer' }}>
            Refresh status
          </button>
          <button type="button" disabled={isWorking} onClick={() => void runAction('simulate-complete')} style={{ padding: '0.8rem 1.1rem', border: 'none', borderRadius: 8, background: '#0f9d58', color: '#fff', cursor: 'pointer' }}>
            Simulate complete
          </button>
        </div>

        {status?.onboardingUrl && (
          <p style={{ marginTop: '1rem', marginBottom: 0 }}>
            Provider onboarding URL: <a href={status.onboardingUrl} target="_blank" rel="noreferrer">{status.onboardingUrl}</a>
          </p>
        )}
      </section>

      <section style={{ marginTop: '1.25rem', padding: '1.2rem', border: '1px solid #e2e6ed', borderRadius: 16, background: '#fff' }}>
        <h2 style={{ marginTop: 0 }}>Verification metadata</h2>
        <div style={{ display: 'grid', gap: '0.5rem' }}>
          <div><strong>Last synced:</strong> {status?.lastSyncedAtUtc ? new Date(status.lastSyncedAtUtc).toLocaleString() : 'Not synced yet'}</div>
          <div><strong>Onboarded at:</strong> {status?.onboardedAtUtc ? new Date(status.onboardedAtUtc).toLocaleString() : 'Not completed'}</div>
        </div>
      </section>
    </main>
  );
};