import React, { useMemo, useState } from 'react';

type Props = {
  isOpen: boolean;
  amount: number;
  projectTitle: string;
  milestoneCount: number;
  milestoneId: string;
  runtimeModeLabel?: string;
  runtimeModeGuidance?: string;
  usesMockResponses?: boolean;
  isSubmitting: boolean;
  submissionError?: string | null;
  onCancel: () => void;
  onSubmit: (payload: { accountName: string; sourceToken: string }) => void;
};

export const PaymentCollectionModal: React.FC<Props> = ({
  isOpen,
  amount,
  projectTitle,
  milestoneCount,
  milestoneId,
  runtimeModeLabel,
  runtimeModeGuidance,
  usesMockResponses,
  isSubmitting,
  submissionError,
  onCancel,
  onSubmit,
}) => {
  const [accountName, setAccountName] = useState('');
  const [sourceToken, setSourceToken] = useState('');
  const [hasAuthority, setHasAuthority] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isValid = useMemo(() => {
    return hasAuthority && accountName.trim().length > 1 && sourceToken.trim().length > 10;
  }, [accountName, hasAuthority, sourceToken]);

  if (!isOpen) {
    return null;
  }

  const submit = () => {
    if (!isValid) {
      setError('Confirm debit authority and enter a valid account name and Pinch source token.');
      return;
    }

    setError(null);
    onSubmit({ accountName: accountName.trim(), sourceToken: sourceToken.trim() });
  };

  return (
    <div style={{ position: 'fixed', inset: 0, background: 'rgba(4, 12, 24, 0.5)', display: 'grid', placeItems: 'center', zIndex: 2000, padding: '1rem' }}>
      <section style={{ width: 'min(720px, 100%)', borderRadius: 20, background: '#fff', border: '1px solid #d9e1ee', boxShadow: '0 24px 80px rgba(0, 0, 0, 0.24)' }}>
        <header style={{ padding: '1.4rem 1.5rem 0.8rem', borderBottom: '1px solid #ecf0f6' }}>
          <p style={{ margin: 0, color: '#0f5ea8', fontWeight: 700 }}>High-volume milestone settlement</p>
          <h2 style={{ margin: '0.35rem 0 0.2rem', fontSize: '1.3rem' }}>Authorise BECS debit for milestone kickoff</h2>
          <p style={{ margin: 0, color: '#4f5b6c' }}>
            Milestone {milestoneId} · {amount.toLocaleString('en-AU', { style: 'currency', currency: 'AUD' })}
          </p>
          <p style={{ margin: '0.4rem 0 0', color: '#4f5b6c' }}>
            Project {projectTitle || 'Untitled project'} · {milestoneCount} milestone{milestoneCount === 1 ? '' : 's'}
          </p>
          <p style={{ margin: '0.4rem 0 0', color: usesMockResponses ? '#9a4b00' : '#0f5ea8', fontWeight: 700 }}>
            Payment mode: {runtimeModeLabel ?? 'Unknown'}
          </p>
        </header>

        <div style={{ padding: '1.2rem 1.5rem 1.4rem', display: 'grid', gap: '0.95rem' }}>
          <label style={{ display: 'grid', gap: '0.4rem' }}>
            <span>Account Name</span>
            <input
              value={accountName}
              onChange={event => setAccountName(event.target.value)}
              placeholder="Coastal SME Services"
              style={{ padding: '0.82rem', borderRadius: 10, border: '1px solid #ccd5e3' }}
            />
          </label>

          <label style={{ display: 'grid', gap: '0.4rem' }}>
            <span>Pinch Source Token</span>
            <input
              value={sourceToken}
              onChange={event => setSourceToken(event.target.value)}
              placeholder="src_..."
              style={{ padding: '0.82rem', borderRadius: 10, border: '1px solid #ccd5e3' }}
            />
          </label>

          <p style={{ margin: 0, color: '#637089', fontSize: '0.9rem' }}>
            This should come from Pinch Capture JS or another tokenization flow so bank details never touch Symbio Hub directly.
          </p>

          <div style={{ margin: 0, padding: '0.75rem 0.9rem', borderRadius: 10, border: '1px solid #d9e4f1', background: '#f7faff', color: '#234567', fontSize: '0.92rem' }}>
            <strong>What happens next:</strong>
            <ol style={{ margin: '0.45rem 0 0.1rem 1.1rem', padding: 0 }}>
              <li>Pinch validates the token and authorization.</li>
              <li>This project is marked escrow-ready for kickoff settlement.</li>
              <li>You can continue to the marketplace or dashboard after confirmation.</li>
            </ol>
          </div>

          <label style={{ display: 'flex', gap: '0.5rem', alignItems: 'flex-start', fontSize: '0.92rem', color: '#32475b' }}>
            <input
              type="checkbox"
              checked={hasAuthority}
              onChange={event => setHasAuthority(event.target.checked)}
              disabled={isSubmitting}
              style={{ marginTop: '0.2rem' }}
            />
            I am authorized to approve direct debit from this account for this project.
          </label>

          {runtimeModeGuidance && (
            <div style={{ margin: 0, padding: '0.75rem 0.9rem', borderRadius: 10, border: `1px solid ${usesMockResponses ? '#f1c089' : '#b9d8f6'}`, background: usesMockResponses ? '#fff6ec' : '#eef7ff', color: usesMockResponses ? '#7e4700' : '#1f4f7a', fontSize: '0.92rem' }}>
              {runtimeModeGuidance}
            </div>
          )}

          {isSubmitting && (
            <div style={{ margin: 0, padding: '0.75rem 0.9rem', borderRadius: 10, border: '1px solid #cfe2cf', background: '#f3fbf3', color: '#246b36', fontWeight: 600, fontSize: '0.92rem' }}>
              Capturing pre-approval with Pinch. Please keep this window open.
            </div>
          )}

          {error && <div style={{ color: '#a80000', fontWeight: 600 }}>{error}</div>}
          {submissionError && <div style={{ color: '#a80000', fontWeight: 600 }}>{submissionError}</div>}

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem' }}>
            <button
              type="button"
              onClick={onCancel}
              disabled={isSubmitting}
              style={{ padding: '0.75rem 1rem', borderRadius: 10, border: '1px solid #ccd5e3', background: '#f8fafc', cursor: 'pointer' }}
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={submit}
              disabled={isSubmitting}
              style={{ padding: '0.75rem 1rem', borderRadius: 10, border: 'none', background: '#006d5b', color: '#fff', fontWeight: 700, cursor: 'pointer' }}
            >
              {isSubmitting ? 'Capturing...' : 'Capture Pre-Approval'}
            </button>
          </div>
        </div>
      </section>
    </div>
  );
};
