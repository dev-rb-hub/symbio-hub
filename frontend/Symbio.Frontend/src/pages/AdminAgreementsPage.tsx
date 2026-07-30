import React, { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiRequest } from '../lib/apiClient';

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
};

type AgreementListResponse = {
  count: number;
  agreements: AgreementRecord[];
};

export const AdminAgreementsPage: React.FC = () => {
  const { session } = useAuth();

  const [searchTerm, setSearchTerm] = useState('');
  const [showPending, setShowPending] = useState(true);
  const [showClosed, setShowClosed] = useState(true);

  const [agreements, setAgreements] = useState<AgreementRecord[]>([]);
  const [selectedAgreementId, setSelectedAgreementId] = useState<number | null>(null);

  const [projectTitle, setProjectTitle] = useState('');
  const [milestoneId, setMilestoneId] = useState('Kickoff');
  const [amount, setAmount] = useState<number>(0);
  const [currency, setCurrency] = useState('AUD');
  const [expertEmail, setExpertEmail] = useState('');
  const [status, setStatus] = useState<AgreementStatus>('PendingApproval');

  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const selectedAgreement = agreements.find(item => item.id === selectedAgreementId) ?? null;

  const statusCounts = useMemo(() => {
    let pending = 0;
    let active = 0;
    let closed = 0;

    agreements.forEach(item => {
      if (item.status === 'PendingApproval') {
        pending += 1;
      } else if (item.status === 'Active') {
        active += 1;
      } else if (item.status === 'Closed') {
        closed += 1;
      }
    });

    return { pending, active, closed };
  }, [agreements]);

  const syncEditorFromSelection = (agreement: AgreementRecord | null) => {
    if (!agreement) {
      setProjectTitle('');
      setMilestoneId('Kickoff');
      setAmount(0);
      setCurrency('AUD');
      setExpertEmail('');
      setStatus('PendingApproval');
      return;
    }

    setProjectTitle(agreement.projectTitle);
    setMilestoneId(agreement.milestoneId);
    setAmount(agreement.amount);
    setCurrency(agreement.currency);
    setExpertEmail(agreement.expertEmail ?? '');
    setStatus(agreement.status);
  };

  const loadAgreements = async (preferredProjectId?: string) => {
    if (!session) {
      return;
    }

    setIsLoading(true);
    setError(null);

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
        syncEditorFromSelection(nextSelected);
      } else if (response.agreements.length > 0) {
        setSelectedAgreementId(response.agreements[0].id);
        syncEditorFromSelection(response.agreements[0]);
      } else {
        setSelectedAgreementId(null);
        syncEditorFromSelection(null);
      }
    } catch {
      setError('Unable to load agreements.');
      setAgreements([]);
      setSelectedAgreementId(null);
      syncEditorFromSelection(null);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (!session) {
      return;
    }

    void loadAgreements();
  }, [session, showPending, showClosed]);

  const onSelectAgreement = (agreement: AgreementRecord) => {
    setSelectedAgreementId(agreement.id);
    syncEditorFromSelection(agreement);
  };

  const handleSearchSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void loadAgreements(selectedAgreement?.projectId);
  };

  const handleSaveCorrections = async () => {
    if (!session || !selectedAgreement) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      await apiRequest('/api/agreements/upsert', {
        method: 'POST',
        token: session.token,
        body: {
          projectId: selectedAgreement.projectId,
          projectTitle,
          milestoneId,
          amount,
          currency,
          expertEmail: expertEmail.trim().length === 0 ? null : expertEmail.trim(),
          status,
        },
      });

      await loadAgreements(selectedAgreement.projectId);
    } catch {
      setError('Unable to save agreement changes.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleRecordApproval = async (targetRole: 'SME' | 'Expert') => {
    if (!session || !selectedAgreement) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      await apiRequest(`/api/agreements/${selectedAgreement.id}/approve`, {
        method: 'POST',
        token: session.token,
        body: {
          targetRole,
        },
      });

      await loadAgreements(selectedAgreement.projectId);
    } catch {
      setError('Unable to record approval for this agreement.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <main className="symbio-page-main">
      <header id="admin-agreements" className="symbio-anchor-target" style={{ borderRadius: 18, padding: '1.2rem 1.3rem', background: 'linear-gradient(130deg, #1b263b 0%, #415a77 55%, #778da9 100%)', color: '#f7fdff', boxShadow: '0 16px 38px rgba(12, 23, 39, 0.24)' }}>
        <p style={{ margin: 0, color: '#dfe9f7', fontWeight: 700 }}>Platform Operations</p>
        <h1 className="symbio-page-title symbio-page-title--dark" style={{ marginBottom: '0.35rem' }}>Admin Agreements Management</h1>
        <p style={{ margin: 0, color: '#dfe9f7' }}>
          Manage agreement records globally and correct relationship data for SME and talent on request.
        </p>
      </header>

      <section style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
        <Link to="/admin/control" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 999, border: '1px solid #d2d8e3', color: '#2d3a4f', background: '#fff' }}>Admin overview</Link>
        <Link to="/agreement" style={{ textDecoration: 'none', padding: '0.5rem 0.75rem', borderRadius: 999, border: '1px solid #d2d8e3', color: '#2d3a4f', background: '#fff' }}>Agreement role view</Link>
      </section>

      {error && <div style={{ marginTop: '0.9rem', color: '#a00' }}>{error}</div>}

      <section style={{ marginTop: '1rem', border: '1px solid #d7dde8', borderRadius: 12, padding: '0.9rem', background: '#f8fbff' }}>
        <form onSubmit={handleSearchSubmit} style={{ display: 'grid', gap: '0.75rem' }}>
          <div style={{ display: 'grid', gap: '0.6rem', gridTemplateColumns: 'minmax(220px, 1fr) auto' }}>
            <input
              value={searchTerm}
              onChange={event => setSearchTerm(event.target.value)}
              placeholder="Search by project, milestone, status, or email"
              style={{ padding: '0.65rem 0.75rem', border: '1px solid #c9d6e8', borderRadius: 8 }}
            />
            <button type="submit" style={{ border: 'none', borderRadius: 8, padding: '0.65rem 0.95rem', background: '#0f5ea8', color: '#fff' }}>
              Search
            </button>
          </div>

          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
            <label style={{ display: 'flex', gap: '0.45rem', alignItems: 'center' }}>
              <input type="checkbox" checked={showPending} onChange={event => setShowPending(event.target.checked)} />
              Show PendingApproval
            </label>
            <label style={{ display: 'flex', gap: '0.45rem', alignItems: 'center' }}>
              <input type="checkbox" checked={showClosed} onChange={event => setShowClosed(event.target.checked)} />
              Show Closed
            </label>
            <button type="button" onClick={() => void loadAgreements(selectedAgreement?.projectId)} style={{ border: '1px solid #c3cedf', borderRadius: 8, padding: '0.6rem 0.85rem', background: '#fff' }}>
              Refresh list
            </button>
          </div>
        </form>
      </section>

      <section style={{ marginTop: '1rem', display: 'grid', gap: '0.8rem', gridTemplateColumns: 'repeat(auto-fit, minmax(170px, 1fr))' }}>
        <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.85rem', background: '#fff' }}>
          <div style={{ color: '#5f6a7d' }}>Loaded agreements</div>
          <strong style={{ fontSize: '1.2rem' }}>{agreements.length}</strong>
        </article>
        <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.85rem', background: '#fff' }}>
          <div style={{ color: '#5f6a7d' }}>PendingApproval</div>
          <strong style={{ fontSize: '1.2rem' }}>{statusCounts.pending}</strong>
        </article>
        <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.85rem', background: '#fff' }}>
          <div style={{ color: '#5f6a7d' }}>Active</div>
          <strong style={{ fontSize: '1.2rem' }}>{statusCounts.active}</strong>
        </article>
        <article style={{ border: '1px solid #d7dde8', borderRadius: 10, padding: '0.85rem', background: '#fff' }}>
          <div style={{ color: '#5f6a7d' }}>Closed</div>
          <strong style={{ fontSize: '1.2rem' }}>{statusCounts.closed}</strong>
        </article>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #d7dde8', borderRadius: 12, overflow: 'hidden', background: '#fff' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.92rem' }}>
          <thead>
            <tr style={{ background: '#f6f9fd', textAlign: 'left' }}>
              <th style={{ padding: '0.6rem 0.75rem' }}>Project</th>
              <th style={{ padding: '0.6rem 0.75rem' }}>SME</th>
              <th style={{ padding: '0.6rem 0.75rem' }}>Expert</th>
              <th style={{ padding: '0.6rem 0.75rem' }}>Status</th>
              <th style={{ padding: '0.6rem 0.75rem' }}>Updated</th>
            </tr>
          </thead>
          <tbody>
            {agreements.map(item => (
              <tr
                key={item.id}
                onClick={() => onSelectAgreement(item)}
                style={{ borderTop: '1px solid #edf2f8', background: selectedAgreementId === item.id ? '#eff8ff' : '#fff', cursor: 'pointer' }}
              >
                <td style={{ padding: '0.55rem 0.75rem' }}>
                  <strong>{item.projectTitle || item.projectId}</strong>
                  <div style={{ color: '#637188' }}>{item.projectId} · {item.milestoneId}</div>
                </td>
                <td style={{ padding: '0.55rem 0.75rem' }}>{item.smeEmail}</td>
                <td style={{ padding: '0.55rem 0.75rem' }}>{item.expertEmail ?? 'Unassigned'}</td>
                <td style={{ padding: '0.55rem 0.75rem' }}>{item.status}</td>
                <td style={{ padding: '0.55rem 0.75rem' }}>{new Date(item.updatedAtUtc).toLocaleString()}</td>
              </tr>
            ))}
            {agreements.length === 0 && !isLoading && (
              <tr>
                <td colSpan={5} style={{ padding: '0.75rem', color: '#637188' }}>No agreements found for current filters.</td>
              </tr>
            )}
          </tbody>
        </table>
      </section>

      <section style={{ marginTop: '1rem', border: '1px solid #d7dde8', borderRadius: 12, padding: '1rem', background: '#fff' }}>
        <h2 style={{ marginTop: 0, color: '#0b4f7f' }}>Edit selected agreement</h2>
        {!selectedAgreement && (
          <p style={{ color: '#637188', marginBottom: 0 }}>Select a row from the table to edit values or correct relationship fields.</p>
        )}

        {selectedAgreement && (
          <div style={{ display: 'grid', gap: '0.8rem' }}>
            <div style={{ color: '#2d3a4f' }}>
              <strong>Project ID:</strong> {selectedAgreement.projectId}
            </div>

            <div style={{ display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
              <label style={{ display: 'grid', gap: '0.3rem' }}>
                <span>Project title</span>
                <input value={projectTitle} onChange={event => setProjectTitle(event.target.value)} style={{ padding: '0.6rem 0.7rem', borderRadius: 8, border: '1px solid #c9d6e8' }} />
              </label>
              <label style={{ display: 'grid', gap: '0.3rem' }}>
                <span>Milestone</span>
                <input value={milestoneId} onChange={event => setMilestoneId(event.target.value)} style={{ padding: '0.6rem 0.7rem', borderRadius: 8, border: '1px solid #c9d6e8' }} />
              </label>
              <label style={{ display: 'grid', gap: '0.3rem' }}>
                <span>Amount</span>
                <input type="number" value={amount} onChange={event => setAmount(Number(event.target.value))} style={{ padding: '0.6rem 0.7rem', borderRadius: 8, border: '1px solid #c9d6e8' }} />
              </label>
              <label style={{ display: 'grid', gap: '0.3rem' }}>
                <span>Currency</span>
                <input value={currency} onChange={event => setCurrency(event.target.value.toUpperCase())} style={{ padding: '0.6rem 0.7rem', borderRadius: 8, border: '1px solid #c9d6e8' }} />
              </label>
              <label style={{ display: 'grid', gap: '0.3rem' }}>
                <span>Expert email</span>
                <input value={expertEmail} onChange={event => setExpertEmail(event.target.value)} placeholder="expert@example.com" style={{ padding: '0.6rem 0.7rem', borderRadius: 8, border: '1px solid #c9d6e8' }} />
              </label>
              <label style={{ display: 'grid', gap: '0.3rem' }}>
                <span>Status</span>
                <select value={status} onChange={event => setStatus(event.target.value as AgreementStatus)} style={{ padding: '0.6rem 0.7rem', borderRadius: 8, border: '1px solid #c9d6e8' }}>
                  <option value="PendingApproval">PendingApproval</option>
                  <option value="Active">Active</option>
                  <option value="Closed">Closed</option>
                </select>
              </label>
            </div>

            <div style={{ display: 'grid', gap: '0.35rem', color: '#425168' }}>
              <div>SME approved: {selectedAgreement.smeApprovedAtUtc ? new Date(selectedAgreement.smeApprovedAtUtc).toLocaleString() : 'Pending'}</div>
              <div>Expert approved: {selectedAgreement.expertApprovedAtUtc ? new Date(selectedAgreement.expertApprovedAtUtc).toLocaleString() : 'Pending'}</div>
              <div>Closed at: {selectedAgreement.closedAtUtc ? new Date(selectedAgreement.closedAtUtc).toLocaleString() : 'Not closed'}</div>
            </div>

            <div style={{ display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
              <button
                type="button"
                disabled={isSaving}
                onClick={() => {
                  void handleSaveCorrections();
                }}
                style={{ padding: '0.65rem 0.95rem', border: 'none', borderRadius: 8, background: '#0f5ea8', color: '#fff' }}
              >
                Save corrections
              </button>
              <button
                type="button"
                disabled={isSaving}
                onClick={() => {
                  void handleRecordApproval('SME');
                }}
                style={{ padding: '0.65rem 0.95rem', border: '1px solid #c3cedf', borderRadius: 8, background: '#fff', color: '#2d3a4f' }}
              >
                Record SME approval
              </button>
              <button
                type="button"
                disabled={isSaving}
                onClick={() => {
                  void handleRecordApproval('Expert');
                }}
                style={{ padding: '0.65rem 0.95rem', border: '1px solid #c3cedf', borderRadius: 8, background: '#fff', color: '#2d3a4f' }}
              >
                Record Expert approval
              </button>
            </div>
          </div>
        )}
      </section>
    </main>
  );
};
