import React from 'react';

export interface PinchRuntimeModeView {
  modeLabel: string;
  environment: string;
  credentialsConfigured: boolean;
  usesMockResponses: boolean;
  portalUrl: string;
  guidance: string;
}

type Props = {
  runtimeMode: PinchRuntimeModeView | null;
  isLoading: boolean;
  hasError: boolean;
};

export const PinchRuntimeModePanel: React.FC<Props> = ({ runtimeMode, isLoading, hasError }) => {
  if (isLoading) {
    return (
      <section style={{ marginTop: '1rem', padding: '0.9rem 1rem', borderRadius: 12, border: '1px solid #d8e3f4', background: '#f7fbff', color: '#254564' }}>
        Checking Pinch runtime mode...
      </section>
    );
  }

  if (hasError || !runtimeMode) {
    return (
      <section style={{ marginTop: '1rem', padding: '0.9rem 1rem', borderRadius: 12, border: '1px solid #f1cfbf', background: '#fff7f2', color: '#7a3d16' }}>
        Pinch runtime mode is unavailable. Continue with caution and verify payment environment settings.
      </section>
    );
  }

  const modeTone = runtimeMode.modeLabel.toLowerCase() === 'live'
    ? { border: '#b8e2c8', background: '#eefbf3', color: '#1a6442' }
    : runtimeMode.modeLabel.toLowerCase() === 'sandbox'
      ? { border: '#b9d8f6', background: '#eef7ff', color: '#1f4f7a' }
      : { border: '#f1c089', background: '#fff6ec', color: '#7e4700' };

  return (
    <section style={{ marginTop: '1rem', padding: '0.9rem 1rem', borderRadius: 12, border: `1px solid ${modeTone.border}`, background: modeTone.background, color: modeTone.color }}>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem', alignItems: 'center' }}>
        <strong>Pinch mode: {runtimeMode.modeLabel}</strong>
        <span>Environment: {runtimeMode.environment}</span>
        <span>Credentials: {runtimeMode.credentialsConfigured ? 'Configured' : 'Missing'}</span>
        <span>Responses: {runtimeMode.usesMockResponses ? 'Simulated' : 'Provider-backed'}</span>
      </div>

      <p style={{ margin: '0.5rem 0 0', color: modeTone.color }}>{runtimeMode.guidance}</p>

      {runtimeMode.portalUrl && (
        <a href={runtimeMode.portalUrl} target="_blank" rel="noreferrer" style={{ display: 'inline-block', marginTop: '0.5rem' }}>
          Open Pinch portal
        </a>
      )}
    </section>
  );
};
