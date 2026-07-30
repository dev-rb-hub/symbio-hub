import React from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { pinchDemoFlow } from '../mocks/pinchLifecycleMock';
import { PaymentLifecycleTimeline } from '../components/payments/PaymentLifecycleTimeline';

export const SettlementCloseoutPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const projectId = searchParams.get('projectId') ?? pinchDemoFlow.projectId;
  const milestoneId = searchParams.get('milestoneId') ?? pinchDemoFlow.milestoneId;

  return (
    <main className="symbio-page-main">
      <header className="symbio-role-hero">
        <p className="symbio-role-hero-kicker">Demo flow · Unified closeout</p>
        <h1 className="symbio-page-title symbio-page-title--dark">Closeout and Settlement Command View</h1>
        <p className="symbio-role-hero-subtitle">
          Single-page summary of evidence completeness, settlement readiness, settlement state, and final reporting for demo and judge walkthroughs.
        </p>
      </header>

      <section style={{ marginTop: '1rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))' }}>
        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '1rem' }}>
          <div style={{ color: '#607084' }}>Project</div>
          <strong>{pinchDemoFlow.projectTitle}</strong>
          <div style={{ marginTop: '0.35rem', color: '#2d3a4f' }}>{projectId} · {milestoneId}</div>
        </article>

        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '1rem' }}>
          <div style={{ color: '#607084' }}>Can settle</div>
          <strong style={{ color: pinchDemoFlow.canSettle ? '#0a6' : '#925200' }}>{pinchDemoFlow.canSettle ? 'Yes' : 'No'}</strong>
          <div style={{ marginTop: '0.35rem', color: '#2d3a4f' }}>{pinchDemoFlow.canSettleReason}</div>
        </article>

        <article style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '1rem' }}>
          <div style={{ color: '#607084' }}>Settlement state</div>
          <strong>{pinchDemoFlow.settlementState}</strong>
          <div style={{ marginTop: '0.35rem', color: '#2d3a4f' }}>Escrow verified: {pinchDemoFlow.escrowVerified ? 'Yes' : 'No'}</div>
        </article>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Evidence completeness</h2>
        <div style={{ display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))' }}>
          <article style={{ border: '1px solid #e4ebf5', borderRadius: 10, padding: '0.8rem' }}>
            <div style={{ color: '#607084' }}>Required</div>
            <strong>{pinchDemoFlow.evidence.totalRequired}</strong>
          </article>
          <article style={{ border: '1px solid #e4ebf5', borderRadius: 10, padding: '0.8rem' }}>
            <div style={{ color: '#607084' }}>Uploaded</div>
            <strong>{pinchDemoFlow.evidence.uploaded}</strong>
          </article>
          <article style={{ border: '1px solid #e4ebf5', borderRadius: 10, padding: '0.8rem' }}>
            <div style={{ color: '#607084' }}>Verified</div>
            <strong>{pinchDemoFlow.evidence.verified}</strong>
          </article>
        </div>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Final reporting snapshot</h2>
        <div style={{ display: 'grid', gap: '0.8rem', gridTemplateColumns: 'repeat(auto-fit, minmax(210px, 1fr))' }}>
          {pinchDemoFlow.reports.map(report => (
            <article key={report.title} style={{ border: '1px solid #e4ebf5', borderRadius: 10, padding: '0.8rem' }}>
              <div style={{ color: '#607084' }}>{report.title}</div>
              <strong>{report.value}</strong>
              <div style={{ marginTop: '0.35rem', color: '#2d3a4f' }}>{report.detail}</div>
            </article>
          ))}
        </div>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #dbe3ef', borderRadius: 12, background: '#f8fbff', padding: '0.95rem 1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Lifecycle confirmation</h2>
        <PaymentLifecycleTimeline events={pinchDemoFlow.timeline} />
      </section>

      <section style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
        <Link to={`/payments/lifecycle?projectId=${encodeURIComponent(projectId)}&milestoneId=${encodeURIComponent(milestoneId)}`} style={{ textDecoration: 'none', padding: '0.6rem 0.9rem', borderRadius: 10, border: '1px solid #d3dbe8', color: '#0a3f66', fontWeight: 700 }}>
          Back to timeline
        </Link>
        <Link to={`/journey?projectId=${encodeURIComponent(projectId)}&stage=closeout`} style={{ textDecoration: 'none', padding: '0.6rem 0.9rem', borderRadius: 10, background: '#0f9d58', color: '#fff', fontWeight: 700 }}>
          Return to journey
        </Link>
      </section>
    </main>
  );
};
