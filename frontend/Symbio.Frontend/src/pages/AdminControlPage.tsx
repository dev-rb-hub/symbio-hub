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

type PinchRuntimeModeResponse = {
  runtimeMode: string;
  environment: string;
  credentialsConfigured: boolean;
  usesMockResponses: boolean;
  baseUri: string;
  authUri: string;
  isLive: boolean;
  guidance: string;
};

type PinchSandboxVerificationResponse = {
  modeLabel: string;
  environment: string;
  credentialsConfigured: boolean;
  connected: boolean;
  merchantReadSucceeded: boolean;
  payerListReadSucceeded: boolean;
  baseUri: string;
  authUri: string;
  isLive: boolean;
  message: string;
  merchantName: string | null;
  failureReason: string | null;
  merchantReadErrorCode: string | null;
  merchantReadErrorMessage: string | null;
  payerListErrorCode: string | null;
  payerListErrorMessage: string | null;
  payerListErrorCount: number;
};

type AdminSection = 'overview' | 'telemetry' | 'compliance' | 'agreements' | 'safety';

const sectionDescriptions: Record<AdminSection, string> = {
  overview: 'Cross-section snapshot for telemetry, compliance workload, and safety overrides.',
  telemetry: 'Platform health indicators and identity/onboarding completion rates.',
  compliance: 'Pending compliance reviews and open project flags requiring action.',
  agreements: 'Global agreement records for relationship correction and status governance.',
  safety: 'Operational safety settings and override controls.',
};

export const AdminControlPage: React.FC = () => {
  const { session } = useAuth();
  const location = useLocation();

  const [telemetry, setTelemetry] = useState<TelemetryResponse | null>(null);
  const [queue, setQueue] = useState<ComplianceQueueResponse | null>(null);
  const [settings, setSettings] = useState<SafetySetting[]>([]);
  const [settingKey, setSettingKey] = useState('');
  const [settingValue, setSettingValue] = useState('');
  const [telemetryError, setTelemetryError] = useState<string | null>(null);
  const [queueError, setQueueError] = useState<string | null>(null);
  const [settingsError, setSettingsError] = useState<string | null>(null);
  const [isTelemetryLoading, setIsTelemetryLoading] = useState(false);
  const [isQueueLoading, setIsQueueLoading] = useState(false);
  const [isSettingsLoading, setIsSettingsLoading] = useState(false);
  const [isSavingSetting, setIsSavingSetting] = useState(false);
  const [resolvingReviewId, setResolvingReviewId] = useState<number | null>(null);
  const [, setPinchDiagnosticsStatus] = useState<'idle' | 'loading' | 'success' | 'warning' | 'error'>('idle');
  const [, setPinchDiagnosticsSummary] = useState('Pinch diagnostics have not run yet.');
  const [pinchDiagnosticsOutput, setPinchDiagnosticsOutput] = useState<string[]>([]);

  const activeSection = useMemo<AdminSection>(() => {
    if (location.pathname.includes('/telemetry')) {
      return 'telemetry';
    }
    if (location.pathname.includes('/compliance')) {
      return 'compliance';
    }
    if (location.pathname.includes('/agreements')) {
      return 'agreements';
    }
    if (location.pathname.includes('/safety')) {
      return 'safety';
    }
    return 'overview';
  }, [location.pathname]);

  const complianceSignal = queue ? queue.pendingReviewCount + queue.openFlagCount : 0;
  const safetySignal = settings.length;

  const loadTelemetry = async () => {
    if (!session) {
      return;
    }

    setIsTelemetryLoading(true);
    setTelemetryError(null);

    try {
      const telemetryData = await apiRequest<TelemetryResponse>('/api/admin/telemetry/global', { token: session.token });
      setTelemetry(telemetryData);
    } catch {
      setTelemetryError('Telemetry is currently unavailable. Verify your admin claim and try again.');
    }

    setIsTelemetryLoading(false);
  };

  const loadQueue = async () => {
    if (!session) {
      return;
    }

    setIsQueueLoading(true);
    setQueueError(null);

    try {
      const queueData = await apiRequest<ComplianceQueueResponse>('/api/admin/compliance/queue', { token: session.token });
      setQueue(queueData);
    } catch {
      setQueueError('Compliance queue is unavailable. Try refreshing this section.');
    }

    setIsQueueLoading(false);
  };

  const loadSettings = async () => {
    if (!session) {
      return;
    }

    setIsSettingsLoading(true);
    setSettingsError(null);

    try {
      const settingsData = await apiRequest<SafetySetting[]>('/api/admin/overrides/safety-settings', { token: session.token });
      setSettings(settingsData);
    } catch {
      setSettingsError('Safety settings are unavailable. Try refreshing this section.');
    }

    setIsSettingsLoading(false);
  };

  const runPinchDiagnostics = async () => {
    if (!session) {
      return;
    }

    const timestamp = new Date().toLocaleTimeString();
    setPinchDiagnosticsStatus('loading');
    setPinchDiagnosticsSummary('Running Pinch diagnostics...');
    setPinchDiagnosticsOutput([
      `[${timestamp}] Starting Pinch runtime and sandbox checks`,
      `[${timestamp}] Requesting runtime mode from /api/payments/runtime-mode`,
      `[${timestamp}] Requesting sandbox verification from /api/payments/pinch/sandbox-verification`,
    ]);

    try {
      const [runtimeMode, sandboxVerification] = await Promise.all([
        apiRequest<PinchRuntimeModeResponse>('/api/payments/runtime-mode', { token: session.token }),
        apiRequest<PinchSandboxVerificationResponse>('/api/payments/pinch/sandbox-verification', { token: session.token }),
      ]);

      const isConfigured = runtimeMode.credentialsConfigured && sandboxVerification.credentialsConfigured;
      const isConnected = sandboxVerification.connected && (sandboxVerification.merchantReadSucceeded || sandboxVerification.payerListReadSucceeded);
      const statusLine = isConfigured && !runtimeMode.usesMockResponses && isConnected
        ? 'Pinch authentication and login checks completed successfully.'
        : runtimeMode.usesMockResponses || !isConfigured
          ? 'Pinch is running in mock or partially configured mode.'
          : 'Pinch sandbox checks failed.';

      const nextOutput = [
        `[${new Date().toLocaleTimeString()}] Runtime mode: ${runtimeMode.runtimeMode ?? 'Unknown'}`,
        `[${new Date().toLocaleTimeString()}] Environment: ${runtimeMode.environment ?? sandboxVerification.environment ?? 'Unknown'}`,
        `[${new Date().toLocaleTimeString()}] Credentials configured: ${isConfigured ? 'yes' : 'no'}`,
        `[${new Date().toLocaleTimeString()}] Connection status: ${isConnected ? 'connected' : 'failed'}`,
        `[${new Date().toLocaleTimeString()}] Merchant read: ${sandboxVerification.merchantReadSucceeded ? 'succeeded' : 'failed'}`,
        `[${new Date().toLocaleTimeString()}] Payer list read: ${sandboxVerification.payerListReadSucceeded ? 'succeeded' : 'failed'}`,
        `[${new Date().toLocaleTimeString()}] ${statusLine}`,
      ];

      setPinchDiagnosticsOutput([...pinchDiagnosticsOutput, ...nextOutput]);
      setPinchDiagnosticsSummary(statusLine);
      setPinchDiagnosticsStatus(isConfigured && !runtimeMode.usesMockResponses && isConnected ? 'success' : runtimeMode.usesMockResponses || !isConfigured ? 'warning' : 'error');
    } catch {
      setPinchDiagnosticsOutput(prev => [...prev, `[${new Date().toLocaleTimeString()}] ERROR: Pinch diagnostics could not be completed.`]);
      setPinchDiagnosticsSummary('Pinch diagnostics failed. Verify the admin token and API availability.');
      setPinchDiagnosticsStatus('error');
    }
  };

  const refreshActiveSection = async () => {
    if (activeSection === 'overview') {
      await Promise.all([loadTelemetry(), loadQueue(), loadSettings()]);
      return;
    }

    if (activeSection === 'telemetry') {
      await loadTelemetry();
      return;
    }

    if (activeSection === 'compliance') {
      await loadQueue();
      return;
    }

    if (activeSection === 'agreements') {
      return;
    }

    await loadSettings();
  };

  useEffect(() => {
    void refreshActiveSection();
    void runPinchDiagnostics();
  }, [session, activeSection]);

  const navLinkStyle = (section: AdminSection): React.CSSProperties => ({
    textDecoration: 'none',
    border: activeSection === section ? '1px solid #0f5ea8' : '1px solid #d2d8e3',
    background: activeSection === section ? '#e8f1fb' : '#fff',
    color: activeSection === section ? '#0f5ea8' : '#2d3a4f',
    borderRadius: 999,
    padding: '0.45rem 0.75rem',
    fontWeight: activeSection === section ? 700 : 500,
  });

  const isSectionBusy =
    (activeSection === 'overview' && (isTelemetryLoading || isQueueLoading || isSettingsLoading))
    || (activeSection === 'telemetry' && isTelemetryLoading)
    || (activeSection === 'compliance' && isQueueLoading)
    || (activeSection === 'safety' && isSettingsLoading);

  const upsertSetting = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!session || !settingKey.trim()) {
      return;
    }

    setIsSavingSetting(true);

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
      await loadSettings();
    } catch {
      setSettingsError('Failed to update safety setting.');
    }

    setIsSavingSetting(false);
  };

  const resolveReview = async (reviewId: number) => {
    if (!session) {
      return;
    }

    setResolvingReviewId(reviewId);

    try {
      await apiRequest(`/api/admin/compliance/reviews/${reviewId}/resolve`, {
        method: 'POST',
        token: session.token,
        body: {
          resolutionNotes: 'Resolved from admin command center',
        },
      });

      await loadQueue();
    } catch {
      setQueueError('Failed to resolve compliance review.');
    }

    setResolvingReviewId(null);
  };

  return (
    <main className="symbio-page-main">
      <header id="admin-dashboard" className="symbio-anchor-target" style={{ borderRadius: 18, padding: '1.2rem 1.3rem', background: 'linear-gradient(130deg, #041a2f 0%, #0f5ea8 55%, #1dcad3 100%)', color: '#f7fdff', boxShadow: '0 18px 46px rgba(3, 22, 40, 0.28)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '0.75rem' }}>
          <div>
            <p style={{ margin: 0, color: '#d8f4ff', fontWeight: 700 }}>Platform Operations</p>
            <h1 className="symbio-page-title symbio-page-title--dark" style={{ marginBottom: '0.25rem' }}>Admin Dashboard</h1>
            <p style={{ margin: 0, color: '#d8f4ff' }}>Welcome back, {session?.email ?? 'Admin'}.</p>
          </div>
        </div>

        <div style={{ marginTop: '0.85rem', display: 'flex', gap: '0.55rem', flexWrap: 'wrap' }}>
          <span style={{ borderRadius: 999, background: 'rgba(255,255,255,0.2)', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Pending reviews: {queue?.pendingReviewCount ?? 0}
          </span>
          <span style={{ borderRadius: 999, background: 'rgba(255,255,255,0.2)', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Open flags: {queue?.openFlagCount ?? 0}
          </span>
          <span style={{ borderRadius: 999, background: 'rgba(255,255,255,0.2)', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Safety overrides: {settings.length}
          </span>
        </div>
      </header>

      <nav style={{ marginTop: '1rem', display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
        <Link to="/admin/control" style={navLinkStyle('overview')}>Overview</Link>
        <Link to="/admin/telemetry" style={navLinkStyle('telemetry')}>Telemetry</Link>
        <Link to="/admin/compliance" style={navLinkStyle('compliance')}>
          Compliance Queue
          {complianceSignal > 0 && (
            <span style={{ marginLeft: '0.4rem', background: '#d44d3f', color: '#fff', borderRadius: 999, padding: '0.05rem 0.45rem', fontSize: '0.78rem', fontWeight: 700 }}>
              {complianceSignal}
            </span>
          )}
        </Link>
        <Link to="/admin/agreements" style={navLinkStyle('agreements')}>Agreements</Link>
        <Link to="/admin/safety" style={navLinkStyle('safety')}>
          Safety Overrides
          {safetySignal > 0 && (
            <span style={{ marginLeft: '0.4rem', background: '#475569', color: '#fff', borderRadius: 999, padding: '0.05rem 0.45rem', fontSize: '0.78rem', fontWeight: 700 }}>
              {safetySignal}
            </span>
          )}
        </Link>
        <button
          type="button"
          onClick={() => void refreshActiveSection()}
          disabled={isSectionBusy}
          style={{ border: '1px solid #bcc6d8', borderRadius: 999, background: '#fff', padding: '0.45rem 0.75rem', cursor: 'pointer' }}
        >
          {isSectionBusy ? 'Refreshing...' : 'Refresh section'}
        </button>
      </nav>

      <section style={{ marginTop: '1rem', border: '1px solid #d7dde8', borderRadius: 12, padding: '0.95rem 1rem', background: '#f8fbff' }}>
        <p style={{ margin: 0, color: '#0f5ea8', fontWeight: 700, textTransform: 'capitalize' }}>Active section: {activeSection}</p>
        <p style={{ margin: '0.4rem 0 0', color: '#425168' }}>{sectionDescriptions[activeSection]}</p>
      </section>

      {activeSection === 'overview' && (
        <section id="admin-overview" className="symbio-anchor-target" style={{ marginTop: '1rem', display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))' }}>
          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.9rem', background: '#fff' }}>
            <h2 style={{ margin: 0, fontSize: '1rem' }}>Telemetry focus</h2>
            <p style={{ margin: '0.45rem 0 0', color: '#5f6a7d' }}>Inspect user health metrics and onboarding conversion trend.</p>
            <Link to="/admin/telemetry" style={{ display: 'inline-block', marginTop: '0.6rem' }}>Open telemetry section</Link>
          </article>

          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.9rem', background: '#fff' }}>
            <h2 style={{ margin: 0, fontSize: '1rem' }}>Compliance focus</h2>
            <p style={{ margin: '0.45rem 0 0', color: '#5f6a7d' }}>
              Pending items: {queue?.pendingReviewCount ?? 0} reviews, {queue?.openFlagCount ?? 0} flags.
            </p>
            <Link to="/admin/compliance" style={{ display: 'inline-block', marginTop: '0.6rem' }}>Open compliance queue</Link>
          </article>

          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.9rem', background: '#fff' }}>
            <h2 style={{ margin: 0, fontSize: '1rem' }}>Safety focus</h2>
            <p style={{ margin: '0.45rem 0 0', color: '#5f6a7d' }}>Configured overrides: {settings.length}</p>
            <Link to="/admin/safety" style={{ display: 'inline-block', marginTop: '0.6rem' }}>Open safety overrides</Link>
          </article>

          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.9rem', background: '#fff' }}>
            <h2 style={{ margin: 0, fontSize: '1rem' }}>Agreement focus</h2>
            <p style={{ margin: '0.45rem 0 0', color: '#5f6a7d' }}>Manage project-level agreement status and party relationships.</p>
            <Link to="/admin/agreements" style={{ display: 'inline-block', marginTop: '0.6rem' }}>Open agreements manager</Link>
          </article>
        </section>
      )}

      {(activeSection === 'overview' || activeSection === 'telemetry') && isTelemetryLoading && (
        <div style={{ marginTop: '1rem', padding: '0.85rem 1rem', background: '#f3f6fa', borderRadius: 10, color: '#3a4a5c' }}>
          Loading telemetry...
        </div>
      )}

      {(activeSection === 'overview' || activeSection === 'telemetry') && telemetryError && (
        <div style={{ marginTop: '1rem', color: '#a00' }}>{telemetryError}</div>
      )}

      {(activeSection === 'overview' || activeSection === 'telemetry') && !isTelemetryLoading && !telemetryError && !telemetry && (
        <article style={{ marginTop: '1rem', border: '1px solid #d7dde8', borderRadius: 10, padding: '0.9rem', background: '#fff', color: '#5f6a7d' }}>
          No telemetry data available yet. Use Refresh section to request the latest platform metrics.
        </article>
      )}

      {(activeSection === 'overview' || activeSection === 'telemetry') && telemetry && (
        <section id="admin-telemetry" className="symbio-anchor-target" style={{ marginTop: '1rem', display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
          <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
            <div style={{ color: '#5f6a7d' }}>Snapshot generated</div>
            <strong style={{ fontSize: '1rem' }}>{new Date(telemetry.generatedAtUtc).toLocaleString()}</strong>
          </article>
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

      {(activeSection === 'overview' || activeSection === 'compliance') && isQueueLoading && (
        <div style={{ marginTop: '1rem', padding: '0.85rem 1rem', background: '#f3f6fa', borderRadius: 10, color: '#3a4a5c' }}>
          Loading compliance queue...
        </div>
      )}

      {(activeSection === 'overview' || activeSection === 'compliance') && queueError && (
        <div style={{ marginTop: '1rem', color: '#a00' }}>{queueError}</div>
      )}

      {(activeSection === 'overview' || activeSection === 'compliance') && !isQueueLoading && !queueError && !queue && (
        <article style={{ marginTop: '1rem', border: '1px solid #d7dde8', borderRadius: 10, padding: '0.9rem', background: '#fff', color: '#5f6a7d' }}>
          No compliance queue data is currently loaded. Use Refresh section to retry.
        </article>
      )}

      {(activeSection === 'overview' || activeSection === 'compliance') && queue && (
        <section id="admin-compliance" className="symbio-anchor-target" style={{ marginTop: '1.2rem', display: 'grid', gap: '0.8rem' }}>
          <h2 style={{ marginBottom: 0 }}>Pending Compliance Reviews</h2>
          {queue.pendingReviews.length === 0 && (
            <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff', color: '#5f6a7d' }}>
              No pending compliance reviews.
            </article>
          )}
          {queue.pendingReviews.map(review => (
            <article key={review.id} style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
              <strong>{review.userEmail}</strong> · {review.userRole} · {review.riskLevel}
              <div style={{ color: '#5f6a7d', marginTop: '0.4rem' }}>{review.notes}</div>
              <button
                onClick={() => void resolveReview(review.id)}
                disabled={resolvingReviewId === review.id}
                style={{ marginTop: '0.65rem', padding: '0.45rem 0.75rem', borderRadius: 8, border: '1px solid #bcc6d8', cursor: 'pointer' }}
              >
                {resolvingReviewId === review.id ? 'Resolving...' : 'Resolve Review'}
              </button>
            </article>
          ))}

          <h2 style={{ marginBottom: 0 }}>Open Project Flags</h2>
          {queue.openFlags.length === 0 && (
            <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff', color: '#5f6a7d' }}>
              No open project flags.
            </article>
          )}
          {queue.openFlags.map(flag => (
            <article key={flag.id} style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '1rem', background: '#fff' }}>
              <strong>{flag.projectId}</strong> · {flag.milestoneId} · {flag.severity}
              <div style={{ color: '#5f6a7d', marginTop: '0.35rem' }}>{flag.reason}</div>
            </article>
          ))}
        </section>
      )}

      {(activeSection === 'overview' || activeSection === 'safety') && isSettingsLoading && (
        <div style={{ marginTop: '1rem', padding: '0.85rem 1rem', background: '#f3f6fa', borderRadius: 10, color: '#3a4a5c' }}>
          Loading safety settings...
        </div>
      )}

      {(activeSection === 'overview' || activeSection === 'safety') && settingsError && (
        <div style={{ marginTop: '1rem', color: '#a00' }}>{settingsError}</div>
      )}

      {(activeSection === 'overview' || activeSection === 'safety') && (
        <section id="admin-safety" className="symbio-anchor-target" style={{ marginTop: '1.2rem' }}>
          <h2>Safety Overrides</h2>
          <form onSubmit={upsertSetting} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr auto', gap: '0.5rem', alignItems: 'center' }}>
            <input value={settingKey} onChange={event => setSettingKey(event.target.value)} placeholder="setting key" style={{ padding: '0.55rem' }} />
            <input value={settingValue} onChange={event => setSettingValue(event.target.value)} placeholder="setting value" style={{ padding: '0.55rem' }} />
            <button type="submit" disabled={isSavingSetting} style={{ padding: '0.55rem 0.85rem', border: '1px solid #bcc6d8', borderRadius: 8 }}>
              {isSavingSetting ? 'Saving...' : 'Save'}
            </button>
          </form>

          <div style={{ marginTop: '0.75rem', display: 'grid', gap: '0.45rem' }}>
            {settings.length === 0 && (
              <article style={{ border: '1px solid #d7dde8', borderRadius: 8, padding: '0.75rem', background: '#fff', color: '#5f6a7d' }}>
                No safety overrides configured yet.
              </article>
            )}
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
