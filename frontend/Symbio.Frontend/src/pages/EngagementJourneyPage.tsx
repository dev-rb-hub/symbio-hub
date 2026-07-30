import React, { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';
import { PinchRuntimeModePanel, PinchRuntimeModeView } from '../components/payments/PinchRuntimeModePanel';

type JourneyStage = {
  title: string;
  summary: string;
  actionLabel: string;
  actionHref: string;
};

const journeyStages: JourneyStage[] = [
  { title: 'Onboard', summary: 'Complete trust profile, confirm identity, and establish who is allowed to operate in the workspace.', actionLabel: 'Trust onboarding', actionHref: '/onboarding' },
  { title: 'Scope', summary: 'Write the project brief, set milestones, and publish a clear engagement boundary.', actionLabel: 'Post project', actionHref: '/project/new' },
  { title: 'Match', summary: 'Shortlist experts by skill and location before moving into negotiation and acceptance.', actionLabel: 'Find talent', actionHref: '/talent/discovery' },
  { title: 'Deliver', summary: 'Use the expert workbench to post progress, validate milestones, and keep visibility aligned.', actionLabel: 'Open workbench', actionHref: '/expert/workbench' },
  { title: 'Settle', summary: 'Track runtime mode, escrow readiness, payment state, and closeout reporting in one place.', actionLabel: 'SME dashboard', actionHref: '/sme/dashboard' },
];

const pinchConcepts = ['Merchant', 'Payer', 'Source', 'Agreement', 'Payment', 'Attempt', 'Transfer', 'Plan', 'Subscription', 'Webhook'];

export const EngagementJourneyPage: React.FC = () => {
  const { session } = useAuth();
  const [searchParams] = useSearchParams();
  const [runtimeMode, setRuntimeMode] = useState<PinchRuntimeModeView | null>(null);
  const [isRuntimeModeLoading, setIsRuntimeModeLoading] = useState(false);
  const [hasRuntimeModeError, setHasRuntimeModeError] = useState(false);

  const projectId = searchParams.get('projectId');
  const stage = searchParams.get('stage') ?? 'journey';

  useEffect(() => {
    if (!session) {
      return;
    }

    setIsRuntimeModeLoading(true);
    setHasRuntimeModeError(false);

    apiRequest<PinchRuntimeModeView>('/api/payments/runtime-mode', {
      token: session.token,
    })
      .then(mode => setRuntimeMode(mode))
      .catch(() => {
        setRuntimeMode(null);
        setHasRuntimeModeError(true);
      })
      .finally(() => {
        setIsRuntimeModeLoading(false);
      });
  }, [session]);

  const role = session?.role ?? 'Guest';
  const nextStep = session?.role === 'Expert'
    ? { label: 'Open escrow onboarding', href: '/escrow/onboarding' }
    : session?.role === 'Admin'
      ? { label: 'Open control center', href: '/admin/control' }
      : { label: 'Start project brief', href: '/project/new' };

  return (
    <main className="symbio-page-main">
      <section style={{ borderRadius: 22, padding: '1.4rem 1.5rem', background: 'linear-gradient(135deg, #031727 0%, #0b4f7f 52%, #11b6c6 100%)', color: '#f8fdff', boxShadow: '0 22px 60px rgba(3, 22, 40, 0.3)' }}>
        <p style={{ margin: 0, opacity: 0.9, fontWeight: 700 }}>Engagement epic</p>
        <h1 className="symbio-page-title symbio-page-title--dark">End-to-end engagement journey</h1>
        <p style={{ margin: 0, maxWidth: 920, lineHeight: 1.65, color: '#d8f4ff' }}>
          One place to move from onboarding and scoping to matching, delivery, settlement, and closeout. The page keeps the Pinch lifecycle visible without scattering the user across disconnected screens.
        </p>

        <div style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
          {session ? (
            <>
              <span style={{ borderRadius: 999, background: 'rgba(255,255,255,0.2)', padding: '0.3rem 0.7rem', fontWeight: 700 }}>{role} workspace</span>
              <Link to={nextStep.href} style={{ textDecoration: 'none', padding: '0.55rem 0.85rem', borderRadius: 10, background: '#fff', color: '#0a3f66', fontWeight: 700 }}>{nextStep.label}</Link>
              <Link to="/sme/dashboard" style={{ textDecoration: 'none', padding: '0.55rem 0.85rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Dashboard</Link>
            </>
          ) : (
            <>
              <span style={{ borderRadius: 999, background: 'rgba(255,255,255,0.2)', padding: '0.3rem 0.7rem', fontWeight: 700 }}>Read only preview</span>
              <Link to="/login" style={{ textDecoration: 'none', padding: '0.55rem 0.85rem', borderRadius: 10, background: '#fff', color: '#0a3f66', fontWeight: 700 }}>Log in to continue</Link>
              <Link to="/onboarding" style={{ textDecoration: 'none', padding: '0.55rem 0.85rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Start onboarding</Link>
            </>
          )}
        </div>
      </section>

      <section style={{ marginTop: '1rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))' }}>
        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1.05rem 1.1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#586375', fontWeight: 700 }}>Current context</div>
          <div style={{ marginTop: '0.35rem', fontSize: '1.05rem', fontWeight: 700, color: '#0a3f66' }}>{projectId ? `Project ${projectId}` : 'No project selected'}</div>
          <div style={{ marginTop: '0.3rem', color: '#2e3b4f' }}>Stage: {stage}</div>
          <div style={{ marginTop: '0.3rem', color: '#2e3b4f' }}>Role: {role}</div>
        </article>

        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1.05rem 1.1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#586375', fontWeight: 700 }}>Next best action</div>
          <div style={{ marginTop: '0.35rem', fontSize: '1.05rem', fontWeight: 700, color: '#0a3f66' }}>{nextStep.label}</div>
          <div style={{ marginTop: '0.3rem', color: '#2e3b4f' }}>Use the journey to jump to the next screen without losing the engagement thread.</div>
        </article>

        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1.05rem 1.1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#586375', fontWeight: 700 }}>Pinch mode</div>
          <div style={{ marginTop: '0.35rem', fontSize: '1.05rem', fontWeight: 700, color: runtimeMode?.usesMockResponses ? '#a65700' : '#0a6' }}>
            {runtimeMode?.modeLabel ?? 'Loading mode'}
          </div>
          <div style={{ marginTop: '0.3rem', color: '#2e3b4f' }}>{runtimeMode?.guidance ?? 'Live Pinch runtime details appear when you are signed in.'}</div>
        </article>
      </section>

      <section style={{ marginTop: '1.25rem' }}>
        <PinchRuntimeModePanel runtimeMode={runtimeMode} isLoading={isRuntimeModeLoading} hasError={hasRuntimeModeError} />
      </section>

      <section style={{ marginTop: '1.4rem' }}>
        <h2 style={{ margin: '0 0 0.75rem', color: '#0b4f7f' }}>Journey stages</h2>
        <div style={{ display: 'grid', gap: '0.9rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
          {journeyStages.map(stageItem => (
            <article key={stageItem.title} style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1rem', boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
              <div style={{ color: '#0f5ea8', fontWeight: 800 }}>{stageItem.title}</div>
              <p style={{ margin: '0.55rem 0 0', color: '#2e3b4f', lineHeight: 1.6 }}>{stageItem.summary}</p>
              <div style={{ marginTop: '0.85rem' }}>
                <Link to={stageItem.actionHref} style={{ textDecoration: 'none', display: 'inline-block', padding: '0.5rem 0.75rem', borderRadius: 10, background: '#0f9d58', color: '#fff', fontWeight: 700 }}>{stageItem.actionLabel}</Link>
              </div>
            </article>
          ))}
        </div>
      </section>

      <section style={{ marginTop: '1.4rem' }}>
        <h2 style={{ margin: '0 0 0.75rem', color: '#0b4f7f' }}>Pinch lifecycle map</h2>
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          {pinchConcepts.map(concept => (
            <span key={concept} style={{ borderRadius: 999, background: '#eff5fb', color: '#2f4c6b', padding: '0.28rem 0.65rem', fontWeight: 700, border: '1px solid #d8e3f4' }}>
              {concept}
            </span>
          ))}
        </div>
        <p style={{ marginTop: '0.9rem', color: '#586375', maxWidth: 920, lineHeight: 1.65 }}>
          This screen keeps the merchant, payer, source, agreement, payment, attempt, transfer, plan, subscription, and webhook concepts visible while the product moves between SME, expert, and admin responsibilities.
        </p>
      </section>

      <section style={{ marginTop: '1.4rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))' }}>
        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1rem' }}>
          <h3 style={{ marginTop: 0, color: '#0b4f7f' }}>SME path</h3>
          <p style={{ color: '#2e3b4f', lineHeight: 1.6 }}>Draft the scope, shortlist experts, launch pre-approval, and monitor settlement from one operating surface.</p>
          <div style={{ display: 'flex', gap: '0.55rem', flexWrap: 'wrap' }}>
            <Link to="/project/new" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 10, background: '#0f9d58', color: '#fff', fontWeight: 700 }}>Project wizard</Link>
            <Link to="/talent/discovery" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 10, border: '1px solid #cfd9e6', color: '#0a3f66', fontWeight: 700 }}>Talent discovery</Link>
          </div>
        </article>

        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1rem' }}>
          <h3 style={{ marginTop: 0, color: '#0b4f7f' }}>Expert path</h3>
          <p style={{ color: '#2e3b4f', lineHeight: 1.6 }}>Complete onboarding, verify escrow readiness, post progress updates, and move the engagement through delivery and reporting.</p>
          <div style={{ display: 'flex', gap: '0.55rem', flexWrap: 'wrap' }}>
            <Link to="/escrow/onboarding" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 10, background: '#0f9d58', color: '#fff', fontWeight: 700 }}>Escrow onboarding</Link>
            <Link to="/expert/workbench" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 10, border: '1px solid #cfd9e6', color: '#0a3f66', fontWeight: 700 }}>Workbench</Link>
          </div>
        </article>

        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 16, padding: '1rem' }}>
          <h3 style={{ marginTop: 0, color: '#0b4f7f' }}>Admin path</h3>
          <p style={{ color: '#2e3b4f', lineHeight: 1.6 }}>Keep runtime, telemetry, compliance, and safety controls visible while the engagement moves through settlement and closure.</p>
          <div style={{ display: 'flex', gap: '0.55rem', flexWrap: 'wrap' }}>
            <Link to="/admin/control" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 10, background: '#0f9d58', color: '#fff', fontWeight: 700 }}>Control center</Link>
            <Link to="/admin/compliance" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 10, border: '1px solid #cfd9e6', color: '#0a3f66', fontWeight: 700 }}>Compliance</Link>
          </div>
        </article>
      </section>
    </main>
  );
};