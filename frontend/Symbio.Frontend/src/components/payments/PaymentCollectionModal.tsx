import React, { useMemo, useState } from 'react';

type Props = {
  isOpen: boolean;
  amount: number;
  milestoneId: string;
  isSubmitting: boolean;
  onCancel: () => void;
  onSubmit: (payload: { accountName: string; bsb: string; accountNumber: string }) => void;
};

function formatBsb(input: string): string {
  const digits = input.replace(/\D/g, '').slice(0, 6);
  if (digits.length <= 3) {
    return digits;
  }

  return `${digits.slice(0, 3)}-${digits.slice(3)}`;
}

function formatAccountNumber(input: string): string {
  return input.replace(/\D/g, '').slice(0, 9);
}

export const PaymentCollectionModal: React.FC<Props> = ({
  isOpen,
  amount,
  milestoneId,
  isSubmitting,
  onCancel,
  onSubmit,
}) => {
  const [accountName, setAccountName] = useState('');
  const [bsb, setBsb] = useState('');
  const [accountNumber, setAccountNumber] = useState('');
  const [error, setError] = useState<string | null>(null);

  const isValid = useMemo(() => {
    return accountName.trim().length > 1 && /^\d{3}-\d{3}$/.test(bsb) && /^\d{3,9}$/.test(accountNumber);
  }, [accountName, bsb, accountNumber]);

  if (!isOpen) {
    return null;
  }

  const submit = () => {
    if (!isValid) {
      setError('Enter a valid account name, BSB, and account number.');
      return;
    }

    setError(null);
    onSubmit({ accountName: accountName.trim(), bsb, accountNumber });
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

          <div style={{ display: 'grid', gap: '0.85rem', gridTemplateColumns: '1fr 1fr' }}>
            <label style={{ display: 'grid', gap: '0.4rem' }}>
              <span>BSB</span>
              <input
                value={bsb}
                onChange={event => setBsb(formatBsb(event.target.value))}
                inputMode="numeric"
                placeholder="123-456"
                style={{ padding: '0.82rem', borderRadius: 10, border: '1px solid #ccd5e3' }}
              />
            </label>

            <label style={{ display: 'grid', gap: '0.4rem' }}>
              <span>Account Number</span>
              <input
                value={accountNumber}
                onChange={event => setAccountNumber(formatAccountNumber(event.target.value))}
                inputMode="numeric"
                placeholder="123456789"
                style={{ padding: '0.82rem', borderRadius: 10, border: '1px solid #ccd5e3' }}
              />
            </label>
          </div>

          <p style={{ margin: 0, color: '#637089', fontSize: '0.9rem' }}>
            This creates a one-time pre-approval token through Pinch so direct debit can be pulled on deployment sign-off without card rails.
          </p>

          {error && <div style={{ color: '#a80000', fontWeight: 600 }}>{error}</div>}

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
