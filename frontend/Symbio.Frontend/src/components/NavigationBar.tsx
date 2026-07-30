import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import symbioLogo from '../assets/images/Symbio-hub.png';

export const NavigationBar: React.FC = () => {
  const { session, logout } = useAuth();

  return (
    <nav className="symbio-nav">
      <div className="symbio-nav-links">
        <Link to="/" className="symbio-brand-link">
          <img src={symbioLogo} alt="Symbio Hub logo" className="symbio-brand-logo" />
          <span>Symbio Hub</span>
        </Link>
        <Link to="/jobs" className="symbio-link">Public jobs</Link>
        <Link to="/marketplace" className="symbio-link">Marketplace</Link>
        {session && session.role === 'SME' && (
          <Link to="/talent/discovery" className="symbio-link">Talent discovery</Link>
        )}
        {session && session.role === 'Expert' && (
          <Link to="/expert/workbench" className="symbio-link">Workbench</Link>
        )}
        {session && session.role === 'Expert' && (
          <Link to="/escrow/onboarding" className="symbio-link">Escrow onboarding</Link>
        )}
        {session && session.role === 'SME' && (
          <Link to="/project/new" className="symbio-link">Post a Project</Link>
        )}
        {session && session.role === 'SME' && (
          <Link to="/billing/control-center" className="symbio-link">Recurring Billing</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/control" className="symbio-link">Operations Hub</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/telemetry" className="symbio-link">Telemetry</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/compliance" className="symbio-link">Compliance Queue</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/safety" className="symbio-link">Safety Overrides</Link>
        )}
        {session && (session.role === 'SME' || session.role === 'Expert') && <Link to="/onboarding" className="symbio-link">Trust onboarding</Link>}
        {session && <Link to="/profile" className="symbio-link">Profile</Link>}
      </div>
      <div className="symbio-nav-account">
        {session ? (
          <>
            <span className="symbio-user-email">{session.email}</span>
            <button onClick={logout} className="symbio-logout-button">
              Log out
            </button>
          </>
        ) : (
          <Link to="/login" className="symbio-link-login">Log in</Link>
        )}
      </div>
    </nav>
  );
};
