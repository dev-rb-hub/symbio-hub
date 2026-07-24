import React from 'react';
import { Link } from 'react-router-dom';

export const LandingPage: React.FC = () => (
  <main style={{ padding: '2rem', fontFamily: 'Arial, sans-serif' }}>
    <section style={{ maxWidth: 900, margin: '0 auto' }}>
      <header>
        <p style={{ color: '#0072ce', fontWeight: 700 }}>Symbio Hub</p>
        <h1 style={{ fontSize: 'clamp(2rem, 4vw, 4rem)', margin: '0.5rem 0' }}>
          Public pitch for regional Australian SMEs and local tech experts
        </h1>
        <p style={{ fontSize: '1.1rem', lineHeight: 1.75, color: '#333' }}>
          Discover high-level jobs, review verified talent signals, and explore the marketplace without exposing any private data or authorization headers.
        </p>
      </header>

      <div style={{ marginTop: '2rem', display: 'grid', gap: '1.25rem' }}>
        <article style={{ padding: '1.5rem', background: '#f5f7fb', borderRadius: 16 }}>
          <h2>For SMEs</h2>
          <p>Browse public opportunities and connect with local digital experts while keeping budgets, contacts, and personal data protected.</p>
        </article>
        <article style={{ padding: '1.5rem', background: '#fff8f1', borderRadius: 16 }}>
          <h2>For Experts</h2>
          <p>Showcase your skills and access regional briefs with compliant, consent-first workflows designed for trusted long-term engagement.</p>
        </article>
      </div>

      <nav style={{ marginTop: '2.5rem', display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
        <Link to="/jobs" style={{ textDecoration: 'none', padding: '0.85rem 1.25rem', background: '#0072ce', color: '#fff', borderRadius: 999 }}>Browse public jobs</Link>
        <Link to="/login" style={{ textDecoration: 'none', padding: '0.85rem 1.25rem', background: '#f1f3f6', color: '#111', borderRadius: 999 }}>Log in</Link>
      </nav>
    </section>
  </main>
);
