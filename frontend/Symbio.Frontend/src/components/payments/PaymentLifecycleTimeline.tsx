import React from 'react';
import { LifecycleEvent } from '../../mocks/pinchLifecycleMock';

type PaymentLifecycleTimelineProps = {
  events: LifecycleEvent[];
};

const statusColor = (status: LifecycleEvent['status']): string => {
  if (status === 'Succeeded') {
    return '#0f9d58';
  }

  if (status === 'Failed') {
    return '#b42318';
  }

  if (status === 'Processing') {
    return '#0f5ea8';
  }

  return '#925200';
};

const typeColor = (type: LifecycleEvent['type']): string => {
  if (type === 'Payment') {
    return '#0f5ea8';
  }

  if (type === 'Attempt') {
    return '#6f4ca5';
  }

  return '#0d7a61';
};

export const PaymentLifecycleTimeline: React.FC<PaymentLifecycleTimelineProps> = ({ events }) => {
  const sorted = [...events].sort((a, b) => new Date(a.atUtc).getTime() - new Date(b.atUtc).getTime());

  return (
    <div style={{ display: 'grid', gap: '0.75rem' }}>
      {sorted.map((event, index) => (
        <article key={event.id} style={{ border: '1px solid #dbe3ef', borderRadius: 12, background: '#fff', padding: '0.9rem 1rem', position: 'relative' }}>
          {index < sorted.length - 1 && (
            <span
              aria-hidden="true"
              style={{
                position: 'absolute',
                left: '1.35rem',
                top: '2.6rem',
                width: 2,
                height: 'calc(100% - 1.4rem)',
                background: '#dbe3ef'
              }}
            />
          )}

          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
            <span
              aria-hidden="true"
              style={{
                marginTop: '0.2rem',
                width: 12,
                height: 12,
                borderRadius: 999,
                background: typeColor(event.type),
                boxShadow: '0 0 0 4px rgba(15, 94, 168, 0.12)'
              }}
            />

            <div style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
                <div>
                  <strong>{event.type}</strong>
                  <div style={{ color: '#607084' }}>{event.reference}</div>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div style={{ fontWeight: 700 }}>
                    {event.amount.toLocaleString('en-AU', { style: 'currency', currency: event.currency })}
                  </div>
                  <div style={{ color: '#607084' }}>{new Date(event.atUtc).toLocaleString()}</div>
                </div>
              </div>

              <div style={{ marginTop: '0.45rem', display: 'flex', gap: '0.55rem', flexWrap: 'wrap', alignItems: 'center' }}>
                <span style={{ borderRadius: 999, background: '#edf3fc', color: '#25537d', border: '1px solid #d4e3f6', padding: '0.1rem 0.5rem', fontWeight: 700, fontSize: '0.82rem' }}>
                  {event.type}
                </span>
                <span style={{ borderRadius: 999, background: '#f7f9fc', color: statusColor(event.status), border: '1px solid #d9e1ed', padding: '0.1rem 0.5rem', fontWeight: 700, fontSize: '0.82rem' }}>
                  {event.status}
                </span>
              </div>

              <p style={{ margin: '0.55rem 0 0', color: '#2d3a4f', lineHeight: 1.55 }}>{event.notes}</p>
            </div>
          </div>
        </article>
      ))}
    </div>
  );
};
