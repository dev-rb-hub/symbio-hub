import React, { useEffect, useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

type TelemetryResponse = {
  generatedAtUtc: string;
  storage: {
    databaseProvider: string;
    sqliteFileBytes: number | null;
    tableRowVolumes: Record<string, number>;
  };
  userProfileHealth: {
    totalUsers: number;
    smeCount: number;
    expertCount: number;
    adminCount: number;
    onboardedCount: number;
    onboardingCompletionRate: number;
    escrowVerifiedCount: number;
    escrowVerificationRate: number;
  };
  regionalProfileHealth: Array<{
    region: string;
    activeProfiles: number;
    pendingCompliance: number;
    openFlags: number;
    healthScore: number;
  }>;
};

type ComplianceQueueResponse = {
  pendingReviewCount: number;
  openFlagCount: number;
  pendingReviews: Array<{
    id: number;
    userEmail: string;
    userRole: string;
    reviewStatus: string;
    riskLevel: string;
    notes: string;
    createdAtUtc: string;
  }>;
  openFlags: Array<{
    id: number;
    projectId: string;
    milestoneId: string;
    severity: string;
    reason: string;
    createdAtUtc: string;
  }>;
};

type SafetySetting = {
  id: number;
  settingKey: string;
  settingValue: string;
  updatedByEmail: string;
  updatedAtUtc: string;
};

export const AdminControlPage: React.FC = () => {
  const { session, logout } = useAuth();
  const location = useLocation();

  const [telemetry, setTelemetry] = useState<TelemetryResponse | null>(null);
  const [queue, setQueue] = useState<ComplianceQueueResponse | null>(null);
  const [settings, setSettings] = useState<SafetySetting[]>([]);
  const [settingKey, setSettingKey] = useState('');
  const [settingValue, setSettingValue] = useState('');
  const [error, setError] = useState<string | null>(null);

  const activeSection = useMemo(() => {
    if (location.pathname.includes('/telemetry')) {
      return 'telemetry';
    }
    if (location.pathname.includes('/compliance')) {
      return 'compliance';
    }
    if (location.pathname.includes('/safety')) {
      return 'safety';
    }
    return 'overview';
  }, [location.pathname]);

  const load = async () => {
    if (!session) {
      return;
    }

    try {
      const [telemetryData, queueData, settingsData] = await Promise.all([
        apiRequest<TelemetryResponse>('/api/admin/telemetry/global', { token: session.token }),
        apiRequest<ComplianceQueueResponse>('/api/admin/compliance/queue', { token: session.token }),
        apiRequest<SafetySetting[]>('/api/admin/overrides/safety-settings', { token: session.token }),
      ]);

      setTelemetry(telemetryData);
      setQueue(queueData);
      setSettings(settingsData);
      setError(null);
    } catch {
      setError('Admin command hub is unavailable. Ensure your account has the admin master claim.');
    }
  };

  useEffect(() => {
    void load();
  }, [session]);

  const upsertSetting = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!session || !settingKey.trim()) {
      return;
    }

    try {
      await apiRequest('/api/admin/overrides/safety-settings', {
        method: 'POST',
        token: session.token,
        body: {
          settingKey,
          settingValue,
        },
      });

      setSettingKey('');
      setSettingValue('');
      await load();
    } catch {
      setError('Failed to update safety setting.');
    }
  };

  const resolveReview = async (reviewId: number) => {
    if (!session) {
      return;
    }

    try {
      await apiRequest(`/api/admin/compliance/reviews/${reviewId}/resolve`, {
        method: 'POST',
        token: session.token,
        body: {
          resolutionNotes: 'Resolved from admin command center',
        },
      });

      await load();
    } catch {
      setError('Failed to resolve compliance review.');
    }
  };

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 1100, margin: '0 auto' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '0.75rem' }}>
        <div>
          <h1 style={{ marginBottom: '0.25rem' }}>Platform Operations Command Hub</h1>
          <p style={{ margin: 0, color: '#566074' }}>Welcome back, {session?.email ?? 'Admin'}.</p>
        </div>
        <button onClick={logout} style={{ padding: '0.65rem 1rem', background: '#c72c41', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer' }}>
          Logout
        </button>
      </header>

      <nav style={{ marginTop: '1rem', display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
        <Link to="/admin/control" style={{ textDecoration: activeSection === 'overview' ? 'underline' : 'none' }}>Overview</Link>
        <Link to="/admin/telemetry" style={{ textDecoration: activeSection === 'telemetry' ? 'underline' : 'none' }}>Telemetry</Link>
        <Link to="/admin/compliance" style={{ textDecoration: activeSection === 'compliance' ? 'underline' : 'none' }}>Compliance Queue</Link>
        <Link to="/admin/safety" style={{ textDecoration: activeSection === 'safety' ? 'underline' : 'none' }}>Safety Overrides</Link>
      </nav>

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      {(activeSection === 'overview' || activeSection === 'telemetry') && telemetry && (
        <section style={{ marginTop: '1rem', display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
            <div style={{ color: '#5f6a7d' }}>Total users</div>
            <strong style={{ fontSize: '1.2rem' }}>{telemetry.userProfileHealth.totalUsers}</strong>
          </article>
          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
            <div style={{ color: '#5f6a7d' }}>Onboarding completion</div>
            <strong style={{ fontSize: '1.2rem' }}>{Math.round(telemetry.userProfileHealth.onboardingCompletionRate * 100)}%</strong>
          </article>
          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
            <div style={{ color: '#5f6a7d' }}>Escrow verification</div>
            <strong style={{ fontSize: '1.2rem' }}>{Math.round(telemetry.userProfileHealth.escrowVerificationRate * 100)}%</strong>
          </article>
          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
            <div style={{ color: '#5f6a7d' }}>DB provider</div>
            <strong style={{ fontSize: '1rem' }}>{telemetry.storage.databaseProvider}</strong>
          </article>
        </section>
      )}

      {(activeSection === 'overview' || activeSection === 'compliance') && queue && (
        <section style={{ marginTop: '1.2rem', display: 'grid', gap: '0.8rem' }}>
          <h2 style={{ marginBottom: 0 }}>Pending Compliance Reviews</h2>
          {queue.pendingReviews.map(review => (
            <article key={review.id} style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
              <strong>{review.userEmail}</strong> · {review.userRole} · {review.riskLevel}
              <div style={{ color: '#5f6a7d', marginTop: '0.4rem' }}>{review.notes}</div>
              <button onClick={() => void resolveReview(review.id)} style={{ marginTop: '0.65rem', padding: '0.45rem 0.75rem', borderRadius: 8, border: '1px solid #bcc6d8', cursor: 'pointer' }}>
                Resolve Review
              </button>
            </article>
          ))}

          <h2 style={{ marginBottom: 0 }}>Open Project Flags</h2>
          {queue.openFlags.map(flag => (
            <article key={flag.id} style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
              <strong>{flag.projectId}</strong> · {flag.milestoneId} · {flag.severity}
              <div style={{ color: '#5f6a7d', marginTop: '0.35rem' }}>{flag.reason}</div>
            </article>
          ))}
        </section>
      )}

      {(activeSection === 'overview' || activeSection === 'safety') && (
        <section style={{ marginTop: '1.2rem' }}>
          <h2>Safety Overrides</h2>
          <form onSubmit={upsertSetting} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr auto', gap: '0.5rem', alignItems: 'center' }}>
            <input value={settingKey} onChange={event => setSettingKey(event.target.value)} placeholder="setting key" style={{ padding: '0.55rem' }} />
            <input value={settingValue} onChange={event => setSettingValue(event.target.value)} placeholder="setting value" style={{ padding: '0.55rem' }} />
            <button type="submit" style={{ padding: '0.55rem 0.85rem', border: '1px solid #bcc6d8', borderRadius: 8 }}>Save</button>
          </form>

          <div style={{ marginTop: '0.75rem', display: 'grid', gap: '0.45rem' }}>
            {settings.map(item => (
              <article key={item.id} style={{ border: '1px solid #d7dde8', borderRadius: 8, padding: '0.75rem', background: '#fff' }}>
                <strong>{item.settingKey}</strong>: {item.settingValue}
                <div style={{ color: '#5f6a7d' }}>Updated by {item.updatedByEmail}</div>
              </article>
            ))}
          </div>
        </section>
      )}
    </main>
  );
};
