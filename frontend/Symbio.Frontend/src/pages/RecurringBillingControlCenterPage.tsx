import React, { useEffect, useState } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useAuth } from '../context/AuthContext';
import { API_BASE_URL, apiRequest } from '../lib/apiClient';
import { PinchRuntimeModePanel, PinchRuntimeModeView } from '../components/payments/PinchRuntimeModePanel';

type MeteredPreview = {
  billableSupportHours: number;
  billableCloudUnits: number;
  supportOverageAmount: number;
  cloudOverageAmount: number;
  totalMeteredAmount: number;
};

type RetainerView = {
  id: number;
  projectId: string;
  milestoneId: string;
  expertEmail: string;
  status: string;
  baseMonthlyAmount: number;
  currency: string;
  includedSupportHours: number;
  includedCloudUnits: number;
  overageRatePerHour: number;
  overageRatePerCloudUnit: number;
  nextBillingAtUtc: string;
  pendingUsageHours: number;
  pendingCloudUnits: number;
  meteredPreview: MeteredPreview;
};

type RetainerCharge = {
  id: number;
  retainerContractId: number;
  providerSubscriptionId: string;
  baseAmount: number;
  meteredAmount: number;
  totalAmount: number;
  currency: string;
  status: string;
  providerReference?: string;
  chargedAtUtc: string;
};

type ControlCenterResponse = {
  clientEmail: string;
  retainers: RetainerView[];
  recentCharges: RetainerCharge[];
};

export const RecurringBillingControlCenterPage: React.FC = () => {
  const { session } = useAuth();
  const [data, setData] = useState<ControlCenterResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [runtimeMode, setRuntimeMode] = useState<PinchRuntimeModeView | null>(null);
  const [isRuntimeModeLoading, setIsRuntimeModeLoading] = useState(false);
  const [hasRuntimeModeError, setHasRuntimeModeError] = useState(false);

  useEffect(() => {
    if (!session) {
      return;
    }

    let active = true;

    setIsRuntimeModeLoading(true);
    setHasRuntimeModeError(false);

    apiRequest<PinchRuntimeModeView>('/api/payments/runtime-mode', {
      token: session.token,
    })
      .then(mode => {
        if (active) {
          setRuntimeMode(mode);
        }
      })
      .catch(() => {
        if (active) {
          setRuntimeMode(null);
          setHasRuntimeModeError(true);
        }
      })
      .finally(() => {
        if (active) {
          setIsRuntimeModeLoading(false);
        }
      });

    const load = async () => {
      try {
        const response = await apiRequest<ControlCenterResponse>('/api/retainers/control-center', {
          token: session.token,
        });

        if (active) {
          setData(response);
          setError(null);
        }
      } catch {
        if (active) {
          setError('Unable to load recurring billing control center.');
        }
      }
    };

    void load();

    const hub = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/accounting`, {
        accessTokenFactory: () => session.token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    hub.on('RetainerChargePosted', () => {
      void load();
    });

    hub.on('RetainerStatusChanged', () => {
      void load();
    });

    void hub.start().then(() => setConnection(hub)).catch(() => {
      if (active) {
        setError('Live retainer stream unavailable.');
      }
    });

    return () => {
      active = false;
      void hub.stop();
    };
  }, [session]);

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 1080, margin: '0 auto' }}>
      <header>
        <p style={{ margin: 0, color: '#0f5ea8', fontWeight: 700 }}>Recurring billing</p>
        <h1 style={{ marginTop: '0.35rem' }}>Maintenance Retainer Control Center</h1>
        <p style={{ color: '#4f5b6c', maxWidth: 820 }}>
          Track monthly retainer subscriptions, included usage coverage, overage previews, and posted recurring BECS charges.
        </p>
      </header>

      <PinchRuntimeModePanel runtimeMode={runtimeMode} isLoading={isRuntimeModeLoading} hasError={hasRuntimeModeError} />

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      <section style={{ marginTop: '1rem', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '0.8rem' }}>
        <article style={{ background: '#fff', border: '1px solid #d8e0ee', borderRadius: 12, padding: '1rem' }}>
          <div style={{ color: '#5d6980' }}>Retainers</div>
          <strong style={{ fontSize: '1.15rem' }}>{data?.retainers.length ?? 0}</strong>
        </article>
        <article style={{ background: '#fff', border: '1px solid #d8e0ee', borderRadius: 12, padding: '1rem' }}>
          <div style={{ color: '#5d6980' }}>Charges posted</div>
          <strong style={{ fontSize: '1.15rem' }}>{data?.recentCharges.length ?? 0}</strong>
        </article>
        <article style={{ background: '#fff', border: '1px solid #d8e0ee', borderRadius: 12, padding: '1rem' }}>
          <div style={{ color: '#5d6980' }}>Live stream</div>
          <strong style={{ fontSize: '1.15rem', color: connection ? '#0a6' : '#a00' }}>{connection ? 'Connected' : 'Connecting'}</strong>
        </article>
      </section>

      <section style={{ marginTop: '1.4rem', display: 'grid', gap: '0.8rem' }}>
        {(data?.retainers ?? []).map(retainer => (
          <article key={retainer.id} style={{ background: '#fff', border: '1px solid #d8e0ee', borderRadius: 12, padding: '1rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
              <div>
                <strong>{retainer.projectId}</strong> · {retainer.milestoneId}
                <div style={{ color: '#5d6980' }}>{retainer.expertEmail}</div>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div>{retainer.baseMonthlyAmount.toLocaleString('en-AU', { style: 'currency', currency: retainer.currency })}/month</div>
                <div style={{ color: '#5d6980' }}>Next billing: {new Date(retainer.nextBillingAtUtc).toLocaleString()}</div>
              </div>
            </div>

            <div style={{ marginTop: '0.75rem', display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              <span><strong>Status:</strong> {retainer.status}</span>
              <span><strong>Pending usage:</strong> {retainer.pendingUsageHours}h, {retainer.pendingCloudUnits} units</span>
              <span><strong>Metered preview:</strong> {retainer.meteredPreview.totalMeteredAmount.toLocaleString('en-AU', { style: 'currency', currency: retainer.currency })}</span>
            </div>
          </article>
        ))}
      </section>
    </main>
  );
};
