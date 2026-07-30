import React, { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

type SettlementReadiness = {
  milestoneId: string;
  canSettle: boolean;
  reason: string;
  evidenceCount: number;
  escrowVerified: boolean;
};

type AgreementStatus = 'PendingApproval' | 'Active' | 'Closed';

type AgreementRecord = {
  id: number;
  projectId: string;
  projectTitle: string;
  milestoneId: string;
  smeUserId: number;
  expertUserId: number | null;
  talentUserId: number | null;
  smeEmail: string;
  expertEmail: string | null;
  amount: number;
  currency: string;
  status: AgreementStatus;
  smeApprovedAtUtc: string | null;
  expertApprovedAtUtc: string | null;
  closedAtUtc: string | null;
  updatedAtUtc: string;
  isCurrentUserProjectOwner?: boolean;
  isCurrentUserTalent?: boolean;
};

type AgreementListResponse = {
  count: number;
  agreements: AgreementRecord[];
};

export const AgreementApprovalPage: React.FC = () => {
  const { session } = useAuth();
  const [searchParams] = useSearchParams();
  const userRole = session?.role ?? 'SME';
  const isProjectOwner = userRole === 'SME';
  const isTalent = userRole === 'Expert';

  const projectId = (searchParams.get('projectId') ?? 'demo-project-epic7-1').trim();
  const projectTitle = (searchParams.get('projectTitle') ?? '').trim();
  const milestoneId = (searchParams.get('milestoneId') ?? 'Kickoff').trim();
  const amount = Number(searchParams.get('amount') ?? '9500');
  const currency = (searchParams.get('currency') ?? 'AUD').trim().toUpperCase();

  const [searchTerm, setSearchTerm] = useState('');
  const [showPending, setShowPending] = useState(false);
  const [showClosed, setShowClosed] = useState(false);
  const [agreements, setAgreements] = useState<AgreementRecord[]>([]);
  const [selectedAgreementId, setSelectedAgreementId] = useState<number | null>(null);
  const [statusUpdateValue, setStatusUpdateValue] = useState<AgreementStatus>('PendingApproval');
  const [adminApprovalTarget, setAdminApprovalTarget] = useState<'SME' | 'Expert'>('SME');
  const [isLoadingAgreements, setIsLoadingAgreements] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [agreementError, setAgreementError] = useState<string | null>(null);
  const [confirmScope, setConfirmScope] = useState(false);
  const [confirmAuthority, setConfirmAuthority] = useState(false);
  const [confirmDebit, setConfirmDebit] = useState(false);
  const [settlementReadiness, setSettlementReadiness] = useState<SettlementReadiness | null>(null);
  const [settlementError, setSettlementError] = useState<string | null>(null);

  const agreementId = useMemo(() => {
    const normalizedProject = projectId.replace(/[^a-zA-Z0-9-]/g, '').toLowerCase();
    const normalizedMilestone = milestoneId.replace(/[^a-zA-Z0-9-]/g, '').toLowerCase();
    return `agr-${normalizedProject}-${normalizedMilestone}`;
  }, [milestoneId, projectId]);

  const selectedAgreement = agreements.find(item => item.id === selectedAgreementId) ?? null;
  const isKickoffReady = selectedAgreement?.status === 'Active';
  const myApprovalAtUtc = isProjectOwner
    ? selectedAgreement?.smeApprovedAtUtc ?? null
    : isTalent
      ? selectedAgreement?.expertApprovedAtUtc ?? null
      : null;

  const loadAgreements = async (preferredProjectId?: string) => {
    if (!session) {
      return;
    }

    setIsLoadingAgreements(true);
    setAgreementError(null);

    const params = new URLSearchParams();
    if (searchTerm.trim().length > 0) {
      params.set('search', searchTerm.trim());
    }

    params.set('includePending', String(showPending));
    params.set('includeClosed', String(showClosed));

    try {
      const response = await apiRequest<AgreementListResponse>(`/api/agreements?${params.toString()}`, {
        token: session.token,
      });

      setAgreements(response.agreements);

      const nextSelected = preferredProjectId
        ? response.agreements.find(item => item.projectId === preferredProjectId)
        : undefined;

      if (nextSelected) {
        setSelectedAgreementId(nextSelected.id);
        setStatusUpdateValue(nextSelected.status);
      } else if (response.agreements.length > 0) {
        setSelectedAgreementId(response.agreements[0].id);
        setStatusUpdateValue(response.agreements[0].status);
      } else {
        setSelectedAgreementId(null);
      }
    } catch {
      setAgreementError('Unable to load agreement records.');
      setAgreements([]);
      setSelectedAgreementId(null);
    } finally {
      setIsLoadingAgreements(false);
    }
  };

  useEffect(() => {
    if (!session) {
      return;
    }

    const syncAgreement = async () => {
      try {
        await apiRequest<AgreementRecord>('/api/agreements/upsert', {
          method: 'POST',
          token: session.token,
          body: {
            projectId,
            projectTitle,
            milestoneId,
            amount,
            currency,
          },
        });
      } catch {
        // A list fetch below will expose user-facing errors if persistence is unavailable.
      }

      await loadAgreements(projectId);
    };

    void syncAgreement();
  }, [session, projectId, projectTitle, milestoneId, amount, currency]);

  useEffect(() => {
    if (!session) {
      return;
    }

    void loadAgreements(selectedAgreement?.projectId);
  }, [showPending, showClosed]);

  useEffect(() => {
    if (!selectedAgreement) {
      return;
    }

    setStatusUpdateValue(selectedAgreement.status);
  }, [selectedAgreement]);

  useEffect(() => {
    if (!session) {
      setSettlementReadiness(null);
      setSettlementError(null);
      return;
    }

    setSettlementError(null);
    apiRequest<SettlementReadiness>(`/api/CompletionEvidence/milestone/${encodeURIComponent(milestoneId)}/can-settle`, {
      token: session.token,
    })
      .then(response => setSettlementReadiness(response))
      .catch(() => {
        setSettlementReadiness(null);
        setSettlementError('Settlement readiness is unavailable until milestone evidence has been captured.');
      });
  }, [milestoneId, session]);

  const canApprove = confirmScope && confirmAuthority && confirmDebit;

  const handleApprove = async () => {
    if (!canApprove || !selectedAgreement || !session) {
      return;
    }

    setIsSaving(true);
    setAgreementError(null);

    try {
      await apiRequest(`/api/agreements/${selectedAgreement.id}/approve`, {
        method: 'POST',
        token: session.token,
        body: {
          targetRole: userRole === 'Admin' ? adminApprovalTarget : undefined,
        },
      });

      await loadAgreements(selectedAgreement.projectId);
    } catch {
      setAgreementError('Unable to record agreement approval.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleStatusUpdate = async () => {
    if (!selectedAgreement || !session) {
      return;
    }

    setIsSaving(true);
    setAgreementError(null);

    try {
      await apiRequest(`/api/agreements/${selectedAgreement.id}/status`, {
        method: 'PATCH',
        token: session.token,
        body: {
          status: statusUpdateValue,
        },
      });

      await loadAgreements(selectedAgreement.projectId);
    } catch {
      setAgreementError('Unable to update agreement status. Ensure both approvals are recorded before setting Active.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleSearchSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void loadAgreements(selectedAgreement?.projectId ?? projectId);
  };

  return (
    <main className="symbio-page-main">
      <header className="symbio-role-hero">
        <p className="symbio-role-hero-kicker">Phase 2 · Agreement</p>
        <h1 className="symbio-page-title symbio-page-title--dark">Engagement Agreement Approval</h1>
        <p className="symbio-role-hero-subtitle">
          Record explicit agreement checks for both project owner and expert before project kickoff and settlement.
        </p>
      </header>

      {agreementError && <div style={{ marginTop: '0.9rem', color: '#a00' }}>{agreementError}</div>}

      <section style={{ marginTop: '1rem', background: '#fff', border: '1px solid #dce5f1', borderRadius: 14, padding: '1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Agreement records</h2>
        <form onSubmit={handleSearchSubmit} style={{ display: 'grid', gap: '0.8rem' }}>
          <div style={{ display: 'grid', gap: '0.7rem', gridTemplateColumns: 'minmax(220px, 1fr) auto' }}>
            <input
              value={searchTerm}
              onChange={event => setSearchTerm(event.target.value)}
              placeholder="Search by project, milestone, email, or status"
              style={{ padding: '0.65rem 0.75rem', border: '1px solid #c9d6e8', borderRadius: 8 }}
            />
            <button type="submit" style={{ padding: '0.65rem 0.95rem', border: 'none', borderRadius: 8, background: '#0a3f66', color: '#fff' }}>
              Search
            </button>
          </div>

          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', color: '#2e3b4f' }}>
            <label style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
              <input type="checkbox" checked={showPending} onChange={event => setShowPending(event.target.checked)} />
              Show PendingApproval
            </label>
            <label style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
              <input type="checkbox" checked={showClosed} onChange={event => setShowClosed(event.target.checked)} />
              Show Closed
            </label>
          </div>
        </form>

        <div style={{ marginTop: '0.9rem', border: '1px solid #e1e8f2', borderRadius: 10, overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.92rem' }}>
            <thead>
              <tr style={{ background: '#f6f9fd', textAlign: 'left' }}>
                <th style={{ padding: '0.6rem 0.7rem' }}>Project</th>
                <th style={{ padding: '0.6rem 0.7rem' }}>SME</th>
                <th style={{ padding: '0.6rem 0.7rem' }}>Expert</th>
                <th style={{ padding: '0.6rem 0.7rem' }}>Status</th>
                <th style={{ padding: '0.6rem 0.7rem' }}>Updated</th>
              </tr>
            </thead>
            <tbody>
              {agreements.map(item => (
                <tr key={item.id} style={{ borderTop: '1px solid #edf2f8', background: selectedAgreementId === item.id ? '#eff8ff' : '#fff', cursor: 'pointer' }} onClick={() => setSelectedAgreementId(item.id)}>
                  <td style={{ padding: '0.55rem 0.7rem' }}>
                    <strong>{item.projectTitle || item.projectId}</strong>
                    <div style={{ color: '#637188' }}>{item.projectId} · {item.milestoneId}</div>
                  </td>
                  <td style={{ padding: '0.55rem 0.7rem' }}>{item.smeEmail}</td>
                  <td style={{ padding: '0.55rem 0.7rem' }}>{item.expertEmail ?? 'Unassigned'}</td>
                  <td style={{ padding: '0.55rem 0.7rem' }}>{item.status}</td>
                  <td style={{ padding: '0.55rem 0.7rem' }}>{new Date(item.updatedAtUtc).toLocaleString()}</td>
                </tr>
              ))}

              {agreements.length === 0 && !isLoadingAgreements && (
                <tr>
                  <td colSpan={5} style={{ padding: '0.75rem', color: '#637188' }}>
                    No agreements match this role scope and filter selection.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section style={{ marginTop: '1rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))' }}>
        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 14, padding: '1rem' }}>
          <div style={{ color: '#586375' }}>Agreement ID</div>
          <strong style={{ fontSize: '1.05rem', color: '#0a3f66' }}>{agreementId}</strong>
          <div style={{ marginTop: '0.35rem', color: '#2e3b4f' }}>Project: {selectedAgreement?.projectId ?? projectId}</div>
          <div style={{ marginTop: '0.2rem', color: '#2e3b4f' }}>Milestone: {selectedAgreement?.milestoneId ?? milestoneId}</div>
        </article>

        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 14, padding: '1rem' }}>
          <div style={{ color: '#586375' }}>Settlement amount</div>
          <strong style={{ fontSize: '1.05rem', color: '#0a3f66' }}>{(selectedAgreement?.amount ?? amount).toLocaleString('en-AU', { style: 'currency', currency: selectedAgreement?.currency ?? currency })}</strong>
          <div style={{ marginTop: '0.35rem', color: '#2e3b4f' }}>Collection: one-time milestone pull</div>
          <div style={{ marginTop: '0.2rem', color: '#2e3b4f' }}>Payer: {selectedAgreement?.smeEmail ?? session?.email ?? 'SME payer'}</div>
        </article>

        <article style={{ background: '#fff', border: '1px solid #dce5f1', borderRadius: 14, padding: '1rem' }}>
          <div style={{ color: '#586375' }}>Kickoff status</div>
          <strong style={{ fontSize: '1.05rem', color: isKickoffReady ? '#0a6' : '#925200' }}>{isKickoffReady ? 'Ready for kickoff' : 'Waiting on agreement checks'}</strong>
          <div style={{ marginTop: '0.35rem', color: '#2e3b4f' }}>
            {myApprovalAtUtc ? `Your check recorded at ${new Date(myApprovalAtUtc).toLocaleString()}` : 'Your agreement checks are not yet recorded'}
          </div>
          <div style={{ marginTop: '0.35rem', color: '#2e3b4f' }}>
            Project owner: {selectedAgreement?.smeApprovedAtUtc ? `Recorded (${new Date(selectedAgreement.smeApprovedAtUtc).toLocaleString()})` : 'Pending'}
          </div>
          <div style={{ marginTop: '0.2rem', color: '#2e3b4f' }}>
            Expert: {selectedAgreement?.expertApprovedAtUtc ? `Recorded (${new Date(selectedAgreement.expertApprovedAtUtc).toLocaleString()})` : 'Pending'}
          </div>
        </article>
      </section>

      <section style={{ marginTop: '1.2rem', background: '#fff', border: '1px solid #dce5f1', borderRadius: 14, padding: '1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Agreement checkpoints</h2>
        <p style={{ color: '#2e3b4f', lineHeight: 1.6 }}>
          Both parties must record approval for scope, authority, and milestone intent before kickoff. Admin users can view and correct records when requested.
        </p>

        <p style={{ marginTop: '0.65rem', marginBottom: '0.25rem', color: '#2e3b4f' }}>
          Signed in as:
          {' '}
          <strong>{userRole}</strong>
          {' '}
          {isProjectOwner ? '(project owner)' : isTalent ? '(delivery expert)' : '(global administrator)'}
        </p>

        <div style={{ display: 'grid', gap: '0.75rem', marginTop: '0.8rem' }}>
          <label style={{ display: 'flex', gap: '0.5rem', alignItems: 'flex-start' }}>
            <input type="checkbox" checked={confirmScope} onChange={event => setConfirmScope(event.target.checked)} />
            <span>I confirm the milestone scope and delivery criteria are accepted for this agreement.</span>
          </label>

          <label style={{ display: 'flex', gap: '0.5rem', alignItems: 'flex-start' }}>
            <input type="checkbox" checked={confirmAuthority} onChange={event => setConfirmAuthority(event.target.checked)} />
            <span>{isProjectOwner ? 'I am authorized to approve this debit agreement for the payer entity.' : 'I accept this agreement as the assigned delivery expert for this milestone.'}</span>
          </label>

          <label style={{ display: 'flex', gap: '0.5rem', alignItems: 'flex-start' }}>
            <input type="checkbox" checked={confirmDebit} onChange={event => setConfirmDebit(event.target.checked)} />
            <span>I acknowledge this can trigger a direct debit pull after settlement prerequisites are satisfied.</span>
          </label>
        </div>

        <div style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
          <button
            type="button"
            disabled={!canApprove || !selectedAgreement || isSaving}
            onClick={() => {
              void handleApprove();
            }}
            style={{ padding: '0.7rem 0.95rem', borderRadius: 8, border: 'none', background: canApprove && selectedAgreement ? '#0f9d58' : '#b6c1cf', color: '#fff', cursor: canApprove && selectedAgreement ? 'pointer' : 'not-allowed', fontWeight: 700 }}
          >
            Record my agreement
          </button>

          {userRole === 'Admin' && (
            <select value={adminApprovalTarget} onChange={event => setAdminApprovalTarget(event.target.value as 'SME' | 'Expert')} style={{ padding: '0.7rem 0.8rem', borderRadius: 8, border: '1px solid #c9d6e8' }}>
              <option value="SME">Approve as SME</option>
              <option value="Expert">Approve as Expert</option>
            </select>
          )}

          <select value={statusUpdateValue} onChange={event => setStatusUpdateValue(event.target.value as AgreementStatus)} style={{ padding: '0.7rem 0.8rem', borderRadius: 8, border: '1px solid #c9d6e8' }}>
            <option value="PendingApproval">PendingApproval</option>
            <option value="Active">Active</option>
            <option value="Closed">Closed</option>
          </select>

          <button
            type="button"
            disabled={!selectedAgreement || isSaving}
            onClick={() => {
              void handleStatusUpdate();
            }}
            style={{ padding: '0.7rem 0.95rem', borderRadius: 8, border: '1px solid #d3dbe8', background: '#fff', color: '#2e3b4f', cursor: 'pointer' }}
          >
            Update status
          </button>
        </div>
      </section>

      <section style={{ marginTop: '1.2rem', background: '#fff', border: '1px solid #dce5f1', borderRadius: 14, padding: '1rem' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Settlement readiness</h2>
        {settlementReadiness ? (
          <>
            <p style={{ margin: '0.35rem 0 0', color: '#2e3b4f' }}>
              Status:
              {' '}
              <strong style={{ color: settlementReadiness.canSettle ? '#0a6' : '#925200' }}>
                {settlementReadiness.canSettle ? 'Ready to settle' : 'Not ready'}
              </strong>
            </p>
            <p style={{ margin: '0.35rem 0 0', color: '#2e3b4f' }}>{settlementReadiness.reason}</p>
            <p style={{ margin: '0.35rem 0 0', color: '#2e3b4f' }}>
              Verified evidence entries: {settlementReadiness.evidenceCount} · Escrow verified: {settlementReadiness.escrowVerified ? 'Yes' : 'No'}
            </p>
          </>
        ) : (
          <p style={{ margin: '0.35rem 0 0', color: '#2e3b4f' }}>
            {settlementError ?? 'Readiness is visible once milestone evidence is posted. Admin can review and support corrections across all agreements.'}
          </p>
        )}
      </section>

      <section style={{ marginTop: '1.2rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
        <Link to={`/journey?projectId=${encodeURIComponent(projectId)}&stage=agreement`} style={{ textDecoration: 'none', padding: '0.65rem 0.9rem', borderRadius: 10, border: '1px solid #d3dbe8', color: '#0a3f66', fontWeight: 700 }}>
          Back to journey
        </Link>
      </section>
    </main>
  );
};
