import React, { useEffect, useMemo, useState } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { API_BASE_URL, apiRequest } from '../lib/apiClient';

interface DeliveryAssignment {
  id: number;
  expertEmail: string;
  projectTitle: string;
  clientName: string;
  category: string;
  scopeSummary: string;
  currentMilestone: string;
  status: string;
  progressPercent: number;
  priority: string;
  dueDate: string;
  isActive: boolean;
  updatedAt: string;
}

interface DeliveryLogEntry {
  id: number;
  deliveryAssignmentId: number;
  expertEmail: string;
  createdByEmail: string;
  level: string;
  message: string;
  createdAt: string;
  progressPercent: number;
  status: string;
  currentMilestone: string;
  projectTitle: string;
}

interface WorkbenchOverview {
  expertEmail: string;
  assignments: DeliveryAssignment[];
  recentLogs: DeliveryLogEntry[];
}

interface EscrowStatus {
  status: string;
}

export const ExpertWorkbenchPage: React.FC = () => {
  const { session, logout } = useAuth();
  const [overview, setOverview] = useState<WorkbenchOverview | null>(null);
  const [selectedAssignmentId, setSelectedAssignmentId] = useState<number | ''>('');
  const [updateMessage, setUpdateMessage] = useState('');
  const [updateLevel, setUpdateLevel] = useState('info');
  const [updateProgress, setUpdateProgress] = useState('');
  const [updateStatus, setUpdateStatus] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isPosting, setIsPosting] = useState(false);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [escrowStatus, setEscrowStatus] = useState<EscrowStatus | null>(null);

  const expertName = useMemo(() => session?.email ?? 'Expert user', [session?.email]);
  const isLoaded = overview !== null;

  useEffect(() => {
    if (!session) {
      return;
    }

    let active = true;

    const loadOverview = async () => {
      try
      {
        const data = await apiRequest<WorkbenchOverview>('/api/expertWorkbench/overview', {
          token: session.token,
        });

        if (active) {
          setOverview(data);
          setSelectedAssignmentId(previous => (previous === '' && data.assignments?.[0]?.id ? data.assignments[0].id : previous));
          setError(null);
        }
      }
      catch
      {
        if (active) {
          setError('Failed to load delivery workbench data.');
        }
        return;
      }
    };

    const loadEscrowStatus = async () => {
      try
      {
        const data = await apiRequest<EscrowStatus>('/api/payments/onboarding/status', {
          token: session.token,
        });

        if (active) {
          setEscrowStatus(data);
        }
      }
      catch
      {
        return;
      }
    };

    void loadOverview();
    void loadEscrowStatus();

    const hub = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/workbench`, {
        accessTokenFactory: () => session.token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    hub.on('WorkbenchLogCreated', (payload: DeliveryLogEntry) => {
      setOverview(current => {
        if (!current) {
          return current;
        }

        const nextLogs = [payload, ...current.recentLogs].slice(0, 20);
        const nextAssignments = current.assignments.map(item => (
          item.id === payload.deliveryAssignmentId
            ? {
                ...item,
                progressPercent: payload.progressPercent ?? item.progressPercent,
                status: payload.status || item.status,
                currentMilestone: payload.currentMilestone || item.currentMilestone,
                updatedAt: payload.createdAt,
              }
            : item
        ));

        return {
          ...current,
          recentLogs: nextLogs,
          assignments: nextAssignments,
        };
      });
    });

    void hub.start().then(() => setConnection(hub)).catch(() => {
      if (active) {
        setError('Live delivery stream could not be connected.');
      }
    });

    return () => {
      active = false;
      void hub.stop();
    };
  }, [session]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!session || selectedAssignmentId === '') {
      return;
    }

    setIsPosting(true);
    setError(null);

    try
    {
      await apiRequest('/api/expertWorkbench/logs', {
        method: 'POST',
        token: session.token,
        body: {
          deliveryAssignmentId: selectedAssignmentId,
          message: updateMessage,
          level: updateLevel,
          progressPercent: updateProgress ? Number(updateProgress) : null,
          status: updateStatus || null,
        },
      });
    }
    catch
    {
      setIsPosting(false);
      setError('Unable to post the workbench update.');
      return;
    }

    setIsPosting(false);

    setUpdateMessage('');
    setUpdateLevel('info');
    setUpdateProgress('');
    setUpdateStatus('');
  };

  if (!session) {
    return null;
  }

  const assignments = overview?.assignments ?? [];
  const logs = overview?.recentLogs ?? [];
  const selectedAssignment = assignments.find(item => item.id === selectedAssignmentId) ?? assignments[0] ?? null;
  const escrowVerified = escrowStatus?.status?.toLowerCase() === 'verified';

  return (
    <main className="symbio-page-main">
      <section style={{ borderRadius: 18, padding: '1.3rem 1.4rem', background: 'linear-gradient(130deg, #041b2f 0%, #0c4f7d 52%, #16c6d5 100%)', color: '#f7fdff', boxShadow: '0 18px 46px rgba(3, 22, 40, 0.28)' }}>
        <p style={{ margin: 0, color: '#d8f4ff', fontWeight: 700 }}>Expert Delivery Workspace</p>
        <h1 className="symbio-page-title symbio-page-title--dark">Delivery Workbench</h1>
        <p style={{ margin: 0, maxWidth: 820, lineHeight: 1.65, color: '#d8f4ff' }}>
          Track active assignments, post progress logs, and keep client visibility synchronized in real time.
        </p>

        <div style={{ marginTop: '0.9rem', display: 'flex', gap: '0.55rem', flexWrap: 'wrap' }}>
          <span style={{ borderRadius: 999, background: connection ? '#0f9d58' : '#8a2f2f', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Stream: {connection ? 'Connected' : 'Connecting'}
          </span>
          <span style={{ borderRadius: 999, background: escrowVerified ? '#0f9d58' : '#a65700', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Escrow: {escrowVerified ? 'Verified' : 'Pending'}
          </span>
          <span style={{ borderRadius: 999, background: 'rgba(255,255,255,0.2)', color: '#fff', padding: '0.25rem 0.65rem', fontSize: '0.86rem', fontWeight: 700 }}>
            Assignments: {assignments.length}
          </span>
        </div>

        <div style={{ marginTop: '1rem', display: 'flex', gap: '0.6rem', flexWrap: 'wrap' }}>
          <Link to="/escrow/onboarding" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, background: '#fff', color: '#0a3f66', fontWeight: 700 }}>Escrow onboarding</Link>
          <Link to="/settings" style={{ textDecoration: 'none', padding: '0.5rem 0.8rem', borderRadius: 10, border: '1px solid rgba(255,255,255,0.55)', color: '#fff', fontWeight: 700 }}>Role settings</Link>
        </div>
      </section>

      {error && <div style={{ marginTop: '1rem', color: '#a00' }}>{error}</div>}

      {!escrowVerified && (
        <section style={{ marginTop: '1rem', padding: '1rem', borderRadius: 12, background: '#fff8f1', border: '1px solid #f3d9b5' }}>
          <strong>Escrow onboarding required:</strong> Complete Pinch Glassbox onboarding before milestone settlement can be requested.
          <div style={{ marginTop: '0.6rem' }}>
            <Link to="/escrow/onboarding" style={{ color: '#a65700', fontWeight: 700, textDecoration: 'none' }}>Open escrow onboarding</Link>
          </div>
        </section>
      )}

      {!isLoaded && !error && <div style={{ marginTop: '1rem', color: '#555' }}>Loading delivery workbench...</div>}

      <section style={{ marginTop: '1.5rem', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
        <article style={{ padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16, boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#555' }}>Signed in as</div>
          <strong style={{ fontSize: '1.05rem' }}>{expertName}</strong>
        </article>
        <article style={{ padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16, boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#555' }}>Assignments</div>
          <strong style={{ fontSize: '1.05rem' }}>{assignments.length}</strong>
        </article>
        <article style={{ padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16, boxShadow: '0 10px 24px rgba(11, 31, 56, 0.06)' }}>
          <div style={{ color: '#555' }}>Live stream</div>
          <strong style={{ fontSize: '1.05rem', color: connection ? '#0a6' : '#a00' }}>{connection ? 'Connected' : 'Connecting'}</strong>
        </article>
      </section>

      <section style={{ marginTop: '1.5rem', display: 'grid', gap: '1rem', gridTemplateColumns: '1.3fr 0.9fr' }}>
        <div style={{ display: 'grid', gap: '1rem' }}>
          {assignments.map(assignment => (
            <article key={assignment.id} style={{ padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
                <div>
                  <h2 style={{ margin: '0 0 0.35rem' }}>{assignment.projectTitle}</h2>
                  <p style={{ margin: 0, color: '#555' }}>{assignment.clientName}</p>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div style={{ fontWeight: 700 }}>{assignment.status}</div>
                  <div style={{ color: '#555' }}>{assignment.priority} priority</div>
                </div>
              </div>

              <p style={{ marginTop: '1rem', lineHeight: 1.65, color: '#333' }}>{assignment.scopeSummary}</p>

              <div style={{ display: 'grid', gap: '0.5rem', marginTop: '1rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', color: '#555' }}>
                  <span>Progress</span>
                  <span>{assignment.progressPercent}%</span>
                </div>
                <div style={{ height: 10, background: '#eef2f7', borderRadius: 999 }}>
                  <div style={{ width: `${assignment.progressPercent}%`, height: '100%', background: '#0f9d58', borderRadius: 999 }} />
                </div>
                <div><strong>Current milestone:</strong> {assignment.currentMilestone}</div>
                <div><strong>Due:</strong> {new Date(assignment.dueDate).toLocaleDateString()}</div>
              </div>

              <button
                type="button"
                onClick={() => setSelectedAssignmentId(assignment.id)}
                style={{ marginTop: '1rem', padding: '0.7rem 1rem', background: selectedAssignmentId === assignment.id ? '#0072ce' : '#f1f3f6', color: selectedAssignmentId === assignment.id ? '#fff' : '#111', border: '1px solid #d6d9dd', borderRadius: 10, cursor: 'pointer' }}
              >
                {selectedAssignmentId === assignment.id ? 'Selected for update' : 'Select for update'}
              </button>
            </article>
          ))}
        </div>

        <aside style={{ display: 'grid', gap: '1rem' }}>
          <form onSubmit={handleSubmit} style={{ padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16, display: 'grid', gap: '1rem' }}>
            <h2 style={{ margin: 0 }}>Post update</h2>
            {assignments.length === 0 ? (
              <p style={{ margin: 0, color: '#555' }}>No active assignments are available yet.</p>
            ) : (
              <>
            <label>
              Assignment
              <select value={selectedAssignmentId} onChange={event => setSelectedAssignmentId(Number(event.target.value))} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }}>
                {assignments.map(assignment => (
                  <option key={assignment.id} value={assignment.id}>{assignment.projectTitle}</option>
                ))}
              </select>
            </label>
            <label>
              Update message
              <textarea value={updateMessage} onChange={event => setUpdateMessage(event.target.value)} rows={4} required style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
            </label>
            <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: '1fr 1fr' }}>
              <label>
                Level
                <select value={updateLevel} onChange={event => setUpdateLevel(event.target.value)} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }}>
                  <option value="info">Info</option>
                  <option value="success">Success</option>
                  <option value="warning">Warning</option>
                  <option value="critical">Critical</option>
                </select>
              </label>
              <label>
                Progress %
                <input value={updateProgress} onChange={event => setUpdateProgress(event.target.value)} type="number" min={0} max={100} placeholder={selectedAssignment ? String(selectedAssignment.progressPercent) : '0'} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
              </label>
            </div>
            <label>
              Status
              <input value={updateStatus} onChange={event => setUpdateStatus(event.target.value)} placeholder={selectedAssignment?.status ?? 'In Progress'} style={{ width: '100%', marginTop: '0.5rem', padding: '0.85rem' }} />
            </label>
            <button type="submit" disabled={isPosting} style={{ padding: '0.85rem 1.25rem', background: '#0f9d58', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer' }}>
              {isPosting ? 'Posting...' : 'Publish work update'}
            </button>
              </>
            )}
          </form>

          <section style={{ padding: '1.25rem', background: '#fff', border: '1px solid #e2e6ed', borderRadius: 16 }}>
            <h2 style={{ marginTop: 0 }}>Live activity</h2>
            <div style={{ display: 'grid', gap: '0.75rem' }}>
              {logs.length === 0 ? (
                <p style={{ margin: 0, color: '#555' }}>No live updates yet.</p>
              ) : (
                logs.map(log => (
                  <article key={log.id} style={{ padding: '0.9rem', background: '#f7f8fb', borderRadius: 12 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem' }}>
                      <strong>{log.projectTitle}</strong>
                      <span style={{ color: '#555' }}>{new Date(log.createdAt).toLocaleTimeString()}</span>
                    </div>
                    <p style={{ margin: '0.5rem 0', lineHeight: 1.6 }}>{log.message}</p>
                    <div style={{ color: '#555', fontSize: '0.95rem' }}>{log.level} · {log.status}</div>
                  </article>
                ))
              )}
            </div>
          </section>
        </aside>
      </section>

      <button onClick={logout} style={{ marginTop: '1.5rem', padding: '0.85rem 1.25rem', background: '#c72c41', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer' }}>
        Logout
      </button>
    </main>
  );
};
