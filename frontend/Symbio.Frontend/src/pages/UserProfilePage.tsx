import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

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

export const UserProfilePage: React.FC = () => {
  const { session } = useAuth();
  const normalizedRole = session?.role?.toLowerCase() ?? '';
  const isAdmin = normalizedRole === 'admin' || normalizedRole.includes('admin');
  const [pinchStatus, setPinchStatus] = useState<'idle' | 'loading' | 'success' | 'warning' | 'error'>('idle');
  const [pinchSummary, setPinchSummary] = useState('No Pinch connectivity check has been run yet.');
  const [pinchOutput, setPinchOutput] = useState<string[]>([]);
  const [pinchDetails, setPinchDetails] = useState<{
    environment: string;
    modeLabel: string;
    credentialsConfigured: boolean;
    connected: boolean;
    merchantReadSucceeded: boolean;
    payerListReadSucceeded: boolean;
    baseUri: string;
    authUri: string;
    guidance: string;
    failureReason: string | null;
  } | null>(null);

  const appendPinchOutput = (message: string) => {
    const timestamp = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    setPinchOutput(previous => [...previous, `[${timestamp}] ${message}`]);
  };

  const runPinchDiagnostics = async () => {
    if (!session || !isAdmin) {
      return;
    }

    setPinchStatus('loading');
    setPinchSummary('Running Pinch connectivity and auth checks...');
    setPinchOutput([]);
    appendPinchOutput('Starting Pinch runtime and sandbox checks');
    appendPinchOutput('Requesting runtime mode from /api/payments/runtime-mode');

    try {
      const runtimeModePromise = apiRequest<PinchRuntimeModeResponse>('/api/payments/runtime-mode', { token: session.token })
        .then(runtimeMode => {
          appendPinchOutput(`Runtime mode resolved: ${runtimeMode.runtimeMode ?? 'unknown'}`);
          return runtimeMode;
        });

      appendPinchOutput('Requesting sandbox verification from /api/payments/pinch/sandbox-verification');
      const sandboxVerificationPromise = apiRequest<PinchSandboxVerificationResponse>('/api/payments/pinch/sandbox-verification', { token: session.token })
        .then(sandboxVerification => {
          appendPinchOutput(`Sandbox verification resolved: ${sandboxVerification.message || 'complete'}`);
          return sandboxVerification;
        });

      const [runtimeMode, sandboxVerification] = await Promise.all([runtimeModePromise, sandboxVerificationPromise]);

      const isConfigured = runtimeMode.credentialsConfigured && sandboxVerification.credentialsConfigured;
      const usedMockResponses = runtimeMode.usesMockResponses;
      const isConnected = sandboxVerification.connected && (sandboxVerification.merchantReadSucceeded || sandboxVerification.payerListReadSucceeded);

      let nextStatus: 'success' | 'warning' | 'error' = 'success';
      let nextSummary = 'Pinch API auth and login checks passed.';

      if (!isConfigured || usedMockResponses) {
        nextStatus = 'warning';
        nextSummary = 'Pinch credentials are not fully configured, so the integration is using mock behavior.';
      } else if (!isConnected) {
        nextStatus = 'error';
        nextSummary = sandboxVerification.failureReason || 'Pinch sandbox login or auth checks failed.';
      }

      appendPinchOutput(`Result: ${nextSummary}`);
      appendPinchOutput(`Environment: ${runtimeMode.environment ?? sandboxVerification.environment ?? 'unknown'}`);
      appendPinchOutput(`Credentials configured: ${isConfigured ? 'yes' : 'no'}`);
      appendPinchOutput(`Sandbox connected: ${sandboxVerification.connected ? 'yes' : 'no'}`);
      appendPinchOutput(`Merchant read: ${sandboxVerification.merchantReadSucceeded ? 'succeeded' : 'failed'}`);
      appendPinchOutput(`Payer list read: ${sandboxVerification.payerListReadSucceeded ? 'succeeded' : 'failed'}`);

      setPinchStatus(nextStatus);
      setPinchSummary(nextSummary);
      setPinchDetails({
        environment: runtimeMode.environment || sandboxVerification.environment || 'Unknown',
        modeLabel: runtimeMode.runtimeMode || sandboxVerification.modeLabel || 'Unknown',
        credentialsConfigured: isConfigured,
        connected: sandboxVerification.connected,
        merchantReadSucceeded: sandboxVerification.merchantReadSucceeded,
        payerListReadSucceeded: sandboxVerification.payerListReadSucceeded,
        baseUri: runtimeMode.baseUri || sandboxVerification.baseUri || 'n/a',
        authUri: runtimeMode.authUri || sandboxVerification.authUri || 'n/a',
        guidance: runtimeMode.guidance || sandboxVerification.message || 'No additional guidance available.',
        failureReason: sandboxVerification.failureReason,
      });
    } catch {
      appendPinchOutput('Error: Pinch diagnostics could not be completed. Verify the admin token and API availability.');
      setPinchStatus('error');
      setPinchSummary('Pinch diagnostics could not be completed. Verify the admin token and API availability.');
      setPinchDetails(null);
    }
  };

  useEffect(() => {
    if (!session || !isAdmin) {
      return;
    }

    void runPinchDiagnostics();
  }, [session, isAdmin]);

  if (!session) {
    return null;
  }

  const roleGuidance = session.role === 'SME'
    ? {
        heading: 'SME Settings',
        summary: 'Manage account identity, onboarding details, and payment workflow destinations.',
        actions: [
          { label: 'Trust onboarding', to: '/onboarding' },
          { label: 'SME dashboard', to: '/sme/dashboard' },
          { label: 'Recurring billing', to: '/billing/control-center' },
        ],
      }
    : session.role === 'Expert'
      ? {
          heading: 'Expert Settings',
          summary: 'Manage account identity and links to onboarding, dashboard, and workbench operations.',
          actions: [
            { label: 'Trust onboarding', to: '/onboarding' },
            { label: 'Expert dashboard', to: '/expert/dashboard' },
            { label: 'Delivery workbench', to: '/expert/workbench' },
          ],
        }
      : {
          heading: 'Admin Settings',
          summary: 'Manage account identity and operational dashboard access points.',
          actions: [
            { label: 'Admin dashboard', to: '/admin/control' },
            { label: 'Compliance queue', to: '/admin/compliance' },
            { label: 'Safety overrides', to: '/admin/safety' },
          ],
        };

  return (
    <main className="symbio-page-main">
      <header className="symbio-role-hero">
        <p className="symbio-role-hero-kicker">Role settings</p>
        <h1 className="symbio-page-title symbio-page-title--dark">{roleGuidance.heading}</h1>
        <p className="symbio-role-hero-subtitle">{roleGuidance.summary}</p>
      </header>
      <dl style={{ display: 'grid', gap: '0.75rem', marginTop: '1.5rem' }}>
        <div><strong>Email:</strong> {session.email}</div>
        <div><strong>Role:</strong> {session.role}</div>
      </dl>

      <section style={{ marginTop: '1.2rem', border: '1px solid #dbe3ef', borderRadius: 12, padding: '1rem', background: '#f8fbff' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.05rem' }}>Role actions</h2>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.55rem' }}>
          {roleGuidance.actions.map(action => (
            <Link key={action.to} to={action.to} style={{ textDecoration: 'none', padding: '0.45rem 0.75rem', borderRadius: 10, border: '1px solid #bdd2ea', background: '#fff', color: '#10436b', fontWeight: 700 }}>
              {action.label}
            </Link>
          ))}
        </div>
      </section>

      {isAdmin && (
        <section style={{ marginTop: '1.2rem', border: '1px solid #dbe3ef', borderRadius: 12, padding: '1rem', background: '#fff' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', gap: '0.8rem', alignItems: 'flex-start', flexWrap: 'wrap' }}>
            <div>
              <h2 style={{ margin: '0 0 0.35rem', fontSize: '1.05rem' }}>Pinch payment API diagnostics</h2>
              <p style={{ margin: 0, color: '#5f6a7d', lineHeight: 1.6 }}>
                Check Pinch auth/login reachability and inspect the current payment environment from the admin settings view.
              </p>
            </div>
            <button
              type="button"
              onClick={() => {
                void runPinchDiagnostics();
              }}
              style={{ padding: '0.55rem 0.8rem', borderRadius: 8, border: '1px solid #bdd2ea', background: '#f8fbff', color: '#10436b', fontWeight: 700, cursor: 'pointer' }}
            >
              {pinchStatus === 'loading' ? 'Checking...' : 'Run Pinch check'}
            </button>
          </div>

          <div style={{ marginTop: '0.9rem', display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
            <span style={{ borderRadius: 999, padding: '0.25rem 0.6rem', background: pinchStatus === 'success' ? '#e8f8ee' : pinchStatus === 'warning' ? '#fff2e8' : '#ffe8e8', color: pinchStatus === 'success' ? '#146c41' : pinchStatus === 'warning' ? '#925200' : '#a12727', fontWeight: 700 }}>
              Status: {pinchStatus === 'success' ? 'Success' : pinchStatus === 'warning' ? 'Warning' : pinchStatus === 'error' ? 'Failure' : 'Idle'}
            </span>
            <span style={{ borderRadius: 999, padding: '0.25rem 0.6rem', background: '#f1f5f9', color: '#475569', fontWeight: 600 }}>
              Environment: {pinchDetails?.environment ?? 'Pending'}
            </span>
            <span style={{ borderRadius: 999, padding: '0.25rem 0.6rem', background: '#f1f5f9', color: '#475569', fontWeight: 600 }}>
              Mode: {pinchDetails?.modeLabel ?? 'Pending'}
            </span>
          </div>

          <p style={{ marginTop: '0.8rem', color: '#334155', lineHeight: 1.6 }}>{pinchSummary}</p>

          {pinchDetails && (
            <div style={{ marginTop: '0.8rem', display: 'grid', gap: '0.55rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
              <div><strong>Credentials configured:</strong> {pinchDetails.credentialsConfigured ? 'Yes' : 'No'}</div>
              <div><strong>Sandbox connection:</strong> {pinchDetails.connected ? 'Connected' : 'Failed'}</div>
              <div><strong>Merchant read:</strong> {pinchDetails.merchantReadSucceeded ? 'Succeeded' : 'Pending/failed'}</div>
              <div><strong>Payer list read:</strong> {pinchDetails.payerListReadSucceeded ? 'Succeeded' : 'Pending/failed'}</div>
              <div><strong>Base URI:</strong> {pinchDetails.baseUri}</div>
              <div><strong>Auth URI:</strong> {pinchDetails.authUri}</div>
              {pinchDetails.failureReason && <div style={{ gridColumn: '1 / -1' }}><strong>Failure reason:</strong> {pinchDetails.failureReason}</div>}
              <div style={{ gridColumn: '1 / -1' }}><strong>Guidance:</strong> {pinchDetails.guidance}</div>
            </div>
          )}

          <div style={{ marginTop: '1rem', border: '1px solid #0d2a3f', borderRadius: 10, background: '#071320', color: '#d7f7ff', padding: '0.8rem', fontFamily: 'Consolas, Monaco, monospace', fontSize: '0.9rem', whiteSpace: 'pre-wrap', overflowX: 'auto' }}>
            <div style={{ marginBottom: '0.45rem', fontWeight: 700, color: '#65e1ff' }}>Pinch diagnostics terminal</div>
            {pinchOutput.length > 0 ? pinchOutput.map((line, index) => (
              <div key={`${line}-${index}`} style={{ marginTop: index === 0 ? 0 : '0.2rem' }}>{line}</div>
            )) : <div>No output yet. Start the diagnostics check to stream progress.</div>}
          </div>
        </section>
      )}
    </main>
  );
};
