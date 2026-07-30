import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const UserProfilePage: React.FC = () => {
  const { session } = useAuth();

  if (!session) {
    return null;
  }

  const roleGuidance = session.role === 'SME'
    ? {
        heading: 'SME settings',
        summary: 'Manage account identity, onboarding details, and payment workflow destinations.',
        actions: [
          { label: 'Trust onboarding', to: '/onboarding' },
          { label: 'SME dashboard', to: '/sme/dashboard' },
          { label: 'Recurring billing', to: '/billing/control-center' },
        ],
      }
    : session.role === 'Expert'
      ? {
          heading: 'Expert settings',
          summary: 'Manage account identity and links to onboarding, dashboard, and workbench operations.',
          actions: [
            { label: 'Trust onboarding', to: '/onboarding' },
            { label: 'Expert dashboard', to: '/expert/dashboard' },
            { label: 'Delivery workbench', to: '/expert/workbench' },
          ],
        }
      : {
          heading: 'Admin settings',
          summary: 'Manage account identity and operational dashboard access points.',
          actions: [
            { label: 'Admin dashboard', to: '/admin/control' },
            { label: 'Compliance queue', to: '/admin/compliance' },
            { label: 'Safety overrides', to: '/admin/safety' },
          ],
        };

  return (
    <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif', maxWidth: 760, margin: '0 auto' }}>
      <h1>{roleGuidance.heading}</h1>
      <p>{roleGuidance.summary}</p>
      <dl style={{ display: 'grid', gap: '0.75rem', marginTop: '1.5rem' }}>
        <div><strong>Email:</strong> {session.email}</div>
        <div><strong>Role:</strong> {session.role}</div>
      </dl>

      <section style={{ marginTop: '1.2rem', border: '1px solid #dbe3ef', borderRadius: 12, padding: '1rem', background: '#f8fbff' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.05rem' }}>Role actions</h2>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.55rem' }}>
          {roleGuidance.actions.map(action => (
            <Link key={action.to} to={action.to} style={{ textDecoration: 'none', padding: '0.45rem 0.75rem', borderRadius: 10, border: '1px solid #bdd2ea', background: '#fff', color: '#10436b', fontWeight: 700 }}>
              {action.label}
            </Link>
          ))}
        </div>
      </section>
    </main>
  );
};
