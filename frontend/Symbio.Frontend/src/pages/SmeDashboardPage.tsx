import React, { useEffect, useMemo, useState } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useAuth } from '../context/AuthContext';
import { API_BASE_URL, apiRequest } from '../lib/apiClient';
import { PinchRuntimeModePanel, PinchRuntimeModeView } from '../components/payments/PinchRuntimeModePanel';

interface AccountingInvoiceView {
  projectId: string;
  milestoneId: string;
  provider: string;
  providerInvoiceId: string;
  invoiceNumber: string;
  invoiceStatus: string;
  paymentState: string;
  totalAmount: number;
  currency: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

interface AccountingInvoiceFeed {
  clientEmail: string;
  count: number;
  invoices: AccountingInvoiceView[];
}

export const SmeDashboardPage: React.FC = () => {
  const { session, logout } = useAuth();
  const [invoices, setInvoices] = useState<AccountingInvoiceView[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [runtimeMode, setRuntimeMode] = useState<PinchRuntimeModeView | null>(null);
  const [isRuntimeModeLoading, setIsRuntimeModeLoading] = useState(false);
  const [hasRuntimeModeError, setHasRuntimeModeError] = useState(false);

  const paidCount = useMemo(() => invoices.filter(item => item.invoiceStatus.toLowerCase() === 'paid').length, [invoices]);

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
      try
      {
        const response = await apiRequest<AccountingInvoiceFeed>('/api/payments/sme/invoices', {
          token: session.token,
        });

        if (active) {
          setInvoices(response.invoices);
          setError(null);
        }
      }
      catch
      {
        if (active) {
          setError('Failed to load accounting invoice feed.');
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

    hub.on('InvoiceStatusChanged', (payload: AccountingInvoiceView) => {
      setInvoices(current => {
        const idx = current.findIndex(item => item.providerInvoiceId === payload.providerInvoiceId);
        if (idx >= 0) {
          const next = [...current];
          next[idx] = {
            ...next[idx],
            ...payload,
          };
          return next;
        }

        return [payload, ...current];
      });
    });

    void hub.start().then(() => setConnection(hub)).catch(() => {
      if (active) {
        setError('Live accounting updates unavailable right now.');
      }
    });

    return () => {
      active = false;
      void hub.stop();
    };
  }, [session]);

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 980, margin: '0 auto' }}>
      <h1>SME Dashboard</h1>
      <p>Welcome back, {session?.email ?? 'SME user'}.</p>
      <p>Automated accounting engine feed for milestone settlement and invoicing.</p>

      <PinchRuntimeModePanel runtimeMode={runtimeMode} isLoading={isRuntimeModeLoading} hasError={hasRuntimeModeError} />

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      <section style={{ marginTop: '1.2rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
        <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 12, padding: '1rem' }}>
          <div style={{ color: '#586375' }}>Invoices synced</div>
          <strong style={{ fontSize: '1.2rem' }}>{invoices.length}</strong>
        </article>
        <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 12, padding: '1rem' }}>
          <div style={{ color: '#586375' }}>Paid</div>
          <strong style={{ fontSize: '1.2rem' }}>{paidCount}</strong>
        </article>
        <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 12, padding: '1rem' }}>
          <div style={{ color: '#586375' }}>Live stream</div>
          <strong style={{ fontSize: '1.2rem', color: connection ? '#0a6' : '#a00' }}>{connection ? 'Connected' : 'Connecting'}</strong>
        </article>
      </section>

      <section style={{ marginTop: '1.5rem', display: 'grid', gap: '0.8rem' }}>
        {invoices.length === 0 && (
          <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 12, padding: '1rem' }}>
            No accounting invoices synced yet.
          </article>
        )}

        {invoices.map(item => (
          <article key={item.providerInvoiceId} style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 12, padding: '1rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
              <div>
                <strong>{item.invoiceNumber}</strong>
                <div style={{ color: '#586375' }}>{item.provider} · {item.providerInvoiceId}</div>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div style={{ fontWeight: 700 }}>{item.totalAmount.toLocaleString('en-AU', { style: 'currency', currency: item.currency })}</div>
                <div style={{ color: '#586375' }}>Project {item.projectId}</div>
              </div>
            </div>
            <div style={{ marginTop: '0.7rem', display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              <span><strong>Invoice:</strong> {item.invoiceStatus}</span>
              <span><strong>Payment:</strong> {item.paymentState}</span>
              <span><strong>Milestone:</strong> {item.milestoneId}</span>
            </div>
          </article>
        ))}
      </section>

      <button onClick={logout} style={{ padding: '0.85rem 1.25rem', background: '#c72c41', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer' }}>
        Logout
      </button>
    </main>
  );
};
