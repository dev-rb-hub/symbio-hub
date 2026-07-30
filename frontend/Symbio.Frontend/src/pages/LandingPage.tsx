import React from 'react';
import { Link } from 'react-router-dom';
import symbioHero from '../assets/images/Symbio-hub-640-320.png';

export const LandingPage: React.FC = () => (
  <main className="symbio-main">
    <section className="symbio-container">
      <header className="symbio-landing-header">
        <img src={symbioHero} alt="Symbio Hub" className="symbio-hero-image" />
        <p className="symbio-kicker">Symbio Hub</p>
        <h1 className="symbio-page-title">
          Public pitch for regional Australian SMEs and local tech experts
        </h1>
        <p className="symbio-intro">
          Discover high-level jobs, review verified talent signals, and explore the marketplace without exposing any private data or authorization headers.
        </p>
      </header>

      <div className="symbio-card-grid">
        <article className="symbio-card symbio-card-a">
          <h2>For SMEs</h2>
          <p>Browse public opportunities and connect with local digital experts while keeping budgets, contacts, and personal data protected.</p>
        </article>
        <article className="symbio-card symbio-card-b">
          <h2>For Experts</h2>
          <p>Showcase your skills, review delivery workbench updates, and access regional briefs with compliant, consent-first workflows designed for trusted long-term engagement.</p>
        </article>
        <article className="symbio-card symbio-card-c">
          <h2>Talent discovery</h2>
          <p>SMEs can search verified expert profiles by skill, location, and summary to shortlist local delivery partners faster.</p>
        </article>
      </div>

      <nav className="symbio-cta-nav">
        <Link to="/jobs" className="symbio-cta symbio-cta-primary">Browse public jobs</Link>
        <Link to="/marketplace" className="symbio-cta symbio-cta-secondary">Explore marketplace</Link>
        <Link to="/talent/discovery" className="symbio-cta symbio-cta-success">Find talent</Link>
        <Link to="/login" className="symbio-cta symbio-cta-secondary">Log in</Link>
        <Link to="/onboarding" className="symbio-cta symbio-cta-success">Start trust onboarding</Link>
      </nav>
    </section>
  </main>
);
