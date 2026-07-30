import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

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
  const [isLoadingStatus, setIsLoadingStatus] = useState(true);
  const [isWorking, setIsWorking] = useState(false);
  const [isPolling, setIsPolling] = useState(false);
  const [pollAttempt, setPollAttempt] = useState(0);

  const loadStatus = async (showLoading = false) => {
    if (!session) {
      return;
    }

    if (showLoading) {
      setIsLoadingStatus(true);
    }

    try
    {
      const data = await apiRequest<EscrowStatus>('/api/payments/onboarding/status', {
        token: session.token,
      });

      setStatus(data);
      setError(null);
    }
    catch
    {
      setError('Failed to load escrow onboarding status.');
      if (showLoading) {
        setIsLoadingStatus(false);
      }
      return;
    }

    if (showLoading) {
      setIsLoadingStatus(false);
    }
  };

  const wait = (milliseconds: number) => new Promise(resolve => {
    window.setTimeout(resolve, milliseconds);
  });

  const pollForVerification = async () => {
    if (!session || isPolling) {
      return;
    }

    setIsPolling(true);
    setPollAttempt(0);

    for (let attempt = 1; attempt <= 15; attempt += 1) {
      await wait(2000);
      setPollAttempt(attempt);

      const statusResult = await apiRequest<EscrowStatus>('/api/payments/onboarding/status', {
        token: session.token,
      });

      setStatus(statusResult);
      const normalizedStatus = statusResult.status.toLowerCase();
      if (normalizedStatus === 'verified') {
        break;
      }
    }

    setIsPolling(false);
  };

  useEffect(() => {
    void loadStatus(true);
  }, [session]);

  const runAction = async (path: 'start' | 'refresh' | 'simulate-complete') => {
    if (!session) {
      return;
    }

    setIsWorking(true);
    try
    {
      const data = await apiRequest<EscrowStatus>(`/api/payments/onboarding/${path}`, {
        method: 'POST',
        token: session.token,
      });

      setStatus(data);
      setError(null);

      if (path !== 'simulate-complete' && data.status.toLowerCase() !== 'verified') {
        void pollForVerification();
      }
    }
    catch
    {
      setIsWorking(false);
      setError('Escrow onboarding action failed.');
      return;
    }

    setIsWorking(false);
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

      {isLoadingStatus && (
        <div style={{ marginTop: '1rem', padding: '1rem 1.1rem', borderRadius: 12, background: '#f3f6fa', color: '#3a4a5c' }}>
          Loading escrow onboarding status...
        </div>
      )}

      {error && (
        <div style={{ marginTop: '0.75rem' }}>
          <button type="button" onClick={() => void loadStatus(true)} style={{ padding: '0.55rem 0.85rem', border: '1px solid #ccd5e3', background: '#fff', borderRadius: 8, cursor: 'pointer' }}>
            Retry
          </button>
        </div>
      )}

      {isPolling && (
        <div style={{ marginTop: '1rem', padding: '0.9rem 1rem', borderRadius: 12, background: '#eef7ff', color: '#134774' }}>
          Awaiting provider verification update... check {pollAttempt}/15.
        </div>
      )}

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
          <button type="button" disabled={isWorking || isPolling} onClick={() => void runAction('start')} style={{ padding: '0.8rem 1.1rem', border: 'none', borderRadius: 8, background: '#0072ce', color: '#fff', cursor: 'pointer' }}>
            Start onboarding
          </button>
          <button type="button" disabled={isWorking || isPolling} onClick={() => void runAction('refresh')} style={{ padding: '0.8rem 1.1rem', border: '1px solid #d6d9dd', borderRadius: 8, background: '#f1f3f6', color: '#111', cursor: 'pointer' }}>
            Refresh status
          </button>
          <button type="button" disabled={isWorking || isPolling} onClick={() => void runAction('simulate-complete')} style={{ padding: '0.8rem 1.1rem', border: 'none', borderRadius: 8, background: '#0f9d58', color: '#fff', cursor: 'pointer' }}>
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