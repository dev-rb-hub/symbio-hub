import React, { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

type DashboardProject = {
  projectTitle: string;
  clientName: string;
  category: string;
  assignmentCount: number;
  progressAverage: number;
  dueSoonest: string;
  statuses: string[];
};

type DashboardMilestone = {
  projectTitle: string;
  milestone: string;
  status: string;
  progressAverage: number;
  priority: string;
  dueDate: string;
  assignmentCount: number;
};

type DashboardPayment = {
  projectTitle: string;
  milestone: string;
  paymentState: string;
  escrowVerified: boolean;
  evidenceCount: number;
  verifiedEvidenceCount: number;
  lastEvidenceAtUtc: string | null;
};

type DashboardReport = {
  id: number;
  deliveryAssignmentId: number;
  projectTitle: string;
  currentMilestone: string;
  level: string;
  message: string;
  createdAt: string;
};

type LevelSummary = {
  level: string;
  count: number;
};

type ExpertDashboardResponse = {
  expertEmail: string;
  totals: {
    projectCount: number;
    milestoneCount: number;
    paymentItemCount: number;
    reportCount: number;
  };
  projects: DashboardProject[];
  milestones: DashboardMilestone[];
  payments: DashboardPayment[];
  reports: DashboardReport[];
  reportLevelSummary: LevelSummary[];
  escrow: {
    status: string;
    escrowVerified: boolean;
    providerAccountId: string;
  };
};

export const ExpertDashboardPage: React.FC = () => {
  const { session } = useAuth();
  const [data, setData] = useState<ExpertDashboardResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [priority, setPriority] = useState('');

  const queryString = useMemo(() => {
    const searchParams = new URLSearchParams();
    if (search.trim()) {
      searchParams.set('search', search.trim());
    }

    if (status.trim()) {
      searchParams.set('status', status.trim());
    }

    if (priority.trim()) {
      searchParams.set('priority', priority.trim());
    }

    searchParams.set('reportLimit', '40');
    return searchParams.toString();
  }, [priority, search, status]);

  useEffect(() => {
    if (!session) {
      return;
    }

    let active = true;
    setIsLoading(true);

    apiRequest<ExpertDashboardResponse>(`/api/expert/dashboard?${queryString}`, {
      token: session.token,
    })
      .then(response => {
        if (active) {
          setData(response);
          setError(null);
        }
      })
      .catch((err: unknown) => {
        if (active) {
          const message = err instanceof Error ? err.message : 'Request failed';

          if (message.includes('404')) {
            setError('Expert dashboard endpoint is unavailable (404). Restart the backend API and confirm /api/expert/dashboard is deployed.');
          } else if (message.includes('401') || message.includes('403')) {
            setError('Access denied for expert dashboard data. Sign in again with an Expert account.');
          } else {
            setError('Unable to load expert dashboard data.');
          }

          setData(null);
        }
      })
      .finally(() => {
        if (active) {
          setIsLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [queryString, session]);

  if (!session) {
    return null;
  }

  const reportSignals = data?.reportLevelSummary ?? [];

  return (
    <main id="dashboard" className="symbio-page-main" style={{ maxWidth: 1220 }}>
      <section style={{ borderRadius: 18, padding: '1.3rem 1.4rem', background: 'linear-gradient(130deg, #031b2f 0%, #0b4f7f 55%, #12bfd6 100%)', color: '#f8fdff', boxShadow: '0 18px 46px rgba(3, 22, 40, 0.28)' }}>
        <p style={{ margin: 0, opacity: 0.9, fontWeight: 700 }}>Expert Delivery Command</p>
        <h1 className="symbio-page-title symbio-page-title--dark">Expert Dashboard</h1>
        <p style={{ margin: 0, maxWidth: 850, lineHeight: 1.6, color: '#d8f4ff' }}>
          Filter and review projects, milestones, payment readiness, and delivery reports from one place before switching into the live workbench.
        </p>

        <div style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
          <Link to="/expert/workbench" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, background: '#fff', color: '#0a3f66', fontWeight: 700 }}>Open workbench</Link>
          <Link to="/escrow/onboarding" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Escrow onboarding</Link>
          <Link to="/payments/lifecycle" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Payment timeline</Link>
          <Link to="/closeout" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Closeout view</Link>
          <Link to="/settings" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Role settings</Link>
        </div>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #d8e3f4', borderRadius: 12, background: '#f7fbff', padding: '0.9rem 1rem' }}>
        <div style={{ display: 'grid', gap: '0.7rem', gridTemplateColumns: '2fr 1fr 1fr auto' }}>
          <input
            value={search}
            onChange={event => setSearch(event.target.value)}
            placeholder="Search project, client, category, or milestone"
            style={{ padding: '0.68rem', borderRadius: 8, border: '1px solid #c5d4e6' }}
          />
          <select value={status} onChange={event => setStatus(event.target.value)} style={{ padding: '0.68rem', borderRadius: 8, border: '1px solid #c5d4e6' }}>
            <option value="">All statuses</option>
            <option value="In Progress">In Progress</option>
            <option value="Under Review">Under Review</option>
            <option value="Blocked">Blocked</option>
            <option value="Done">Done</option>
          </select>
          <select value={priority} onChange={event => setPriority(event.target.value)} style={{ padding: '0.68rem', borderRadius: 8, border: '1px solid #c5d4e6' }}>
            <option value="">All priorities</option>
            <option value="High">High</option>
            <option value="Medium">Medium</option>
            <option value="Low">Low</option>
          </select>
          <button type="button" onClick={() => { setSearch(''); setStatus(''); setPriority(''); }} style={{ padding: '0.68rem 0.9rem', borderRadius: 8, border: '1px solid #c5d4e6', background: '#fff', cursor: 'pointer' }}>
            Clear
          </button>
        </div>
      </section>

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}
      {isLoading && <div style={{ marginTop: '1rem', color: '#566074' }}>Loading expert dashboard...</div>}

      {!isLoading && data && (
        <>
          <section style={{ marginTop: '1rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <div style={{ color: '#586375' }}>Projects</div>
              <strong style={{ fontSize: '1.3rem' }}>{data.totals.projectCount}</strong>
            </article>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <div style={{ color: '#586375' }}>Milestones</div>
              <strong style={{ fontSize: '1.3rem' }}>{data.totals.milestoneCount}</strong>
            </article>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <div style={{ color: '#586375' }}>Payment items</div>
              <strong style={{ fontSize: '1.3rem' }}>{data.totals.paymentItemCount}</strong>
            </article>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <div style={{ color: '#586375' }}>Escrow</div>
              <strong style={{ fontSize: '1.3rem', color: data.escrow.escrowVerified ? '#0a6' : '#a65700' }}>{data.escrow.status}</strong>
            </article>
          </section>

          <section id="projects" style={{ marginTop: '1.4rem' }}>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <h2 style={{ marginTop: 0 }}>Projects</h2>
              {(data.projects.length === 0) && <p style={{ color: '#586375' }}>No projects match the current filters.</p>}
              <div style={{ display: 'grid', gap: '0.7rem' }}>
                {data.projects.map(project => (
                  <div key={`${project.projectTitle}-${project.clientName}`} style={{ border: '1px solid #e6ebf4', borderRadius: 10, padding: '0.7rem' }}>
                    <strong>{project.projectTitle}</strong>
                    <div style={{ color: '#586375' }}>{project.clientName} · {project.category}</div>
                    <div style={{ marginTop: '0.35rem', color: '#2e3b4f' }}>Assignments: {project.assignmentCount} · Avg progress: {project.progressAverage}%</div>
                  </div>
                ))}
              </div>
            </article>
          </section>

          <section id="milestones" style={{ marginTop: '1rem' }}>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <h2 style={{ marginTop: 0 }}>Milestones</h2>
              {(data.milestones.length === 0) && <p style={{ color: '#586375' }}>No milestones match the current filters.</p>}
              <div style={{ display: 'grid', gap: '0.7rem' }}>
                {data.milestones.map(item => (
                  <div key={`${item.projectTitle}-${item.milestone}`} style={{ border: '1px solid #e6ebf4', borderRadius: 10, padding: '0.7rem' }}>
                    <strong>{item.milestone}</strong>
                    <div style={{ color: '#586375' }}>{item.projectTitle}</div>
                    <div style={{ marginTop: '0.35rem', color: '#2e3b4f' }}>Status: {item.status} · Priority: {item.priority} · Progress: {item.progressAverage}%</div>
                  </div>
                ))}
              </div>
            </article>
          </section>

          <section id="payments" style={{ marginTop: '1rem' }}>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <h2 style={{ marginTop: 0 }}>Payments</h2>
              {(data.payments.length === 0) && <p style={{ color: '#586375' }}>No payment readiness records yet.</p>}
              <div style={{ display: 'grid', gap: '0.7rem' }}>
                {data.payments.map(payment => (
                  <div key={`${payment.projectTitle}-${payment.milestone}`} style={{ border: '1px solid #e6ebf4', borderRadius: 10, padding: '0.7rem' }}>
                    <strong>{payment.projectTitle}</strong>
                    <div style={{ color: '#586375' }}>{payment.milestone}</div>
                    <div style={{ marginTop: '0.35rem' }}>
                      <span style={{ borderRadius: 999, padding: '0.15rem 0.5rem', background: payment.paymentState === 'SettlementReady' ? '#e8f8ee' : payment.paymentState === 'EscrowPending' ? '#fff2e8' : '#eef4fb', color: payment.paymentState === 'SettlementReady' ? '#146c41' : payment.paymentState === 'EscrowPending' ? '#925200' : '#25537d', fontWeight: 700 }}>
                        {payment.paymentState}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </article>
          </section>

          <section id="reports" style={{ marginTop: '1rem' }}>
            <article style={{ background: '#fff', border: '1px solid #dde3ee', borderRadius: 14, padding: '1rem' }}>
              <h2 style={{ marginTop: 0 }}>Reports</h2>
              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '0.7rem' }}>
                {reportSignals.map(signal => (
                  <span key={signal.level} style={{ borderRadius: 999, background: '#eff3f9', color: '#31455f', padding: '0.2rem 0.55rem', fontWeight: 700, fontSize: '0.86rem' }}>
                    {signal.level}: {signal.count}
                  </span>
                ))}
              </div>

              {(data.reports.length === 0) && <p style={{ color: '#586375' }}>No reports available for current filters.</p>}
              <div style={{ display: 'grid', gap: '0.7rem', maxHeight: 420, overflow: 'auto', paddingRight: '0.3rem' }}>
                {data.reports.map(report => (
                  <div key={report.id} style={{ border: '1px solid #e6ebf4', borderRadius: 10, padding: '0.7rem' }}>
                    <strong>{report.projectTitle}</strong>
                    <div style={{ color: '#586375' }}>{report.currentMilestone}</div>
                    <p style={{ margin: '0.35rem 0', lineHeight: 1.55 }}>{report.message}</p>
                    <div style={{ color: '#586375', fontSize: '0.9rem' }}>{report.level} · {new Date(report.createdAt).toLocaleString()}</div>
                  </div>
                ))}
              </div>
            </article>
          </section>
        </>
      )}
    </main>
  );
};
