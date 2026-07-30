import React from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { PaymentLifecycleTimeline } from '../components/payments/PaymentLifecycleTimeline';
import { pinchDemoFlow } from '../mocks/pinchLifecycleMock';
import { useAuth } from '../context/AuthContext';

export const PaymentLifecycleDemoPage: React.FC = () => {
  const { session } = useAuth();
  const [searchParams] = useSearchParams();
  const projectId = searchParams.get('projectId') ?? pinchDemoFlow.projectId;
  const milestoneId = searchParams.get('milestoneId') ?? pinchDemoFlow.milestoneId;

  return (
    <main className="symbio-page-main">
      <header className="symbio-role-hero">
        <p className="symbio-role-hero-kicker">Demo flow · Payment lifecycle</p>
        <h1 className="symbio-page-title symbio-page-title--dark">Payment, Attempt, and Transfer Timeline</h1>
        <p className="symbio-role-hero-subtitle">
          Mock timeline for judge walkthroughs showing the full settlement sequence from agreement approval through transfer processing.
        </p>
      </header>

      <section style={{ marginTop: '1rem', display: 'grid', gap: '0.8rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '0.95rem' }}>
          <div style={{ color: '#607084' }}>Signed-in role</div>
          <strong>{session?.role ?? 'Guest'}</strong>
        </article>
        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '0.95rem' }}>
          <div style={{ color: '#607084' }}>Project</div>
          <strong>{pinchDemoFlow.projectTitle}</strong>
          <div style={{ marginTop: '0.25rem', color: '#2d3a4f' }}>{projectId}</div>
        </article>
        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '0.95rem' }}>
          <div style={{ color: '#607084' }}>Milestone</div>
          <strong>{milestoneId}</strong>
        </article>
        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '0.95rem' }}>
          <div style={{ color: '#607084' }}>Agreement status</div>
          <strong style={{ color: pinchDemoFlow.agreementStatus === 'Active' ? '#0a6' : '#925200' }}>{pinchDemoFlow.agreementStatus}</strong>
        </article>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #dbe3ef', borderRadius: 12, background: '#f8fbff', padding: '0.95rem 1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Timeline stream</h2>
        <p style={{ margin: '0 0 0.85rem', color: '#2d3a4f' }}>
          Shared lifecycle view for SME, Expert, and Admin to reason over payment progression and where manual intervention would be required.
        </p>
        <PaymentLifecycleTimeline events={pinchDemoFlow.timeline} />
      </section>

      <section style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
        <Link to={`/agreement?projectId=${encodeURIComponent(projectId)}&milestoneId=${encodeURIComponent(milestoneId)}`} style={{ textDecoration: 'none', padding: '0.6rem 0.9rem', borderRadius: 10, border: '1px solid #d3dbe8', color: '#0a3f66', fontWeight: 700 }}>
          Back to agreement
        </Link>
        <Link to={`/closeout?projectId=${encodeURIComponent(projectId)}&milestoneId=${encodeURIComponent(milestoneId)}`} style={{ textDecoration: 'none', padding: '0.6rem 0.9rem', borderRadius: 10, background: '#0f9d58', color: '#fff', fontWeight: 700 }}>
          Open unified closeout
        </Link>
      </section>
    </main>
  );
};
