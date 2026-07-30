import React, { useEffect, useMemo, useState } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Link } from 'react-router-dom';
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
  const pendingCount = useMemo(() => invoices.filter(item => item.invoiceStatus.toLowerCase() !== 'paid').length, [invoices]);
  const totalInvoiced = useMemo(() => invoices.reduce((sum, item) => sum + item.totalAmount, 0), [invoices]);

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
    <main className="symbio-page-main">
      <section id="sme-dashboard" className="symbio-anchor-target" style={{ borderRadius: 18, padding: '1.3rem 1.4rem', background: 'linear-gradient(130deg, #031b2f 0%, #0b4f7f 55%, #12bfd6 100%)', color: '#f8fdff', boxShadow: '0 18px 46px rgba(3, 22, 40, 0.28)' }}>
        <p style={{ margin: 0, opacity: 0.9, fontWeight: 700 }}>SME Financial Command</p>
        <h1 className="symbio-page-title symbio-page-title--dark">SME Dashboard</h1>
        <p style={{ margin: 0, maxWidth: 760, lineHeight: 1.6, color: '#d8f4ff' }}>
          Welcome back, {session?.email ?? 'SME user'}. Track invoicing health, payment state transitions, and runtime readiness from one control surface.
        </p>

        <div style={{ marginTop: '0.9rem', display: 'flex', flexWrap: 'wrap', gap: '0.55rem' }}>
          <span style={{ borderRadius: 999, background: connection ? '#0f9d58' : '#8a2f2f', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Stream: {connection ? 'Connected' : 'Connecting'}
          </span>
          <span style={{ borderRadius: 999, background: 'rgba(255, 255, 255, 0.2)', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Paid {paidCount}/{invoices.length || 0}
          </span>
        </div>

        <div style={{ marginTop: '1rem', display: 'flex', flexWrap: 'wrap', gap: '0.6rem' }}>
          <Link to="/talent/discovery" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Talent discovery</Link>
          <Link to="/project/new" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, background: '#fff', color: '#0a3f66', fontWeight: 700 }}>Post new project</Link>
          <Link to="/billing/control-center" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Recurring billing</Link>
        </div>
      </section>

      <section id="sme-runtime" className="symbio-anchor-target" style={{ marginTop: '1.2rem' }}>
        <h2 style={{ margin: '0 0 0.75rem', color: '#0b4f7f' }}>Runtime</h2>
        <PinchRuntimeModePanel runtimeMode={runtimeMode} isLoading={isRuntimeModeLoading} hasError={hasRuntimeModeError} />
      </section>

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      <section id="sme-summary" className="symbio-anchor-target" style={{ marginTop: '1.2rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
        <h2 style={{ margin: 0, color: '#0b4f7f', gridColumn: '1 / -1' }}>Summary</h2>
        <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#586375' }}>Invoices synced</div>
          <strong style={{ fontSize: '1.35rem' }}>{invoices.length}</strong>
        </article>
        <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#586375' }}>Total invoiced</div>
          <strong style={{ fontSize: '1.35rem' }}>{totalInvoiced.toLocaleString('en-AU', { style: 'currency', currency: 'AUD' })}</strong>
        </article>
        <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#586375' }}>Pending invoices</div>
          <strong style={{ fontSize: '1.35rem', color: pendingCount > 0 ? '#aa4d00' : '#0a6' }}>{pendingCount}</strong>
        </article>
      </section>

      <section id="sme-invoices" className="symbio-anchor-target" style={{ marginTop: '1.5rem', display: 'grid', gap: '0.8rem' }}>
        <h2 style={{ margin: '0 0 0.35rem', color: '#0b4f7f' }}>Invoices</h2>
        {invoices.length === 0 && (
          <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 12, padding: '1rem' }}>
            No accounting invoices synced yet.
          </article>
        )}

        {invoices.map(item => {
          const isPaid = item.invoiceStatus.toLowerCase() === 'paid';
          return (
            <article key={item.providerInvoiceId} style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
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
              <div style={{ marginTop: '0.7rem', display: 'flex', gap: '0.65rem', flexWrap: 'wrap' }}>
                <span style={{ borderRadius: 999, padding: '0.2rem 0.55rem', background: isPaid ? '#e8f8ee' : '#fff2e8', color: isPaid ? '#146c41' : '#925200', fontWeight: 700 }}>
                  Invoice {item.invoiceStatus}
                </span>
                <span><strong>Payment:</strong> {item.paymentState}</span>
                <span><strong>Milestone:</strong> {item.milestoneId}</span>
              </div>
            </article>
          );
        })}
      </section>

      <button onClick={logout} style={{ marginTop: '1.4rem', padding: '0.85rem 1.25rem', background: '#c72c41', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer' }}>
        Logout
      </button>
    </main>
  );
};
