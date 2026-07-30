import React, { useEffect, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import symbioLogo from '../assets/images/Symbio-hub.png';

export const NavigationBar: React.FC = () => {
  const { session, logout } = useAuth();
  const location = useLocation();
  const showExpertDashboardMenu = session?.role === 'Expert' && location.pathname === '/expert/dashboard';
  const [isDashboardMenuOpen, setIsDashboardMenuOpen] = useState(showExpertDashboardMenu);

  useEffect(() => {
    if (!showExpertDashboardMenu) {
      setIsDashboardMenuOpen(false);
      return;
    }

    setIsDashboardMenuOpen(true);
  }, [showExpertDashboardMenu]);

  return (
    <nav className="symbio-nav">
      <div className="symbio-nav-links">
        <div className="symbio-brand-menu-wrap">
          <Link to="/" className="symbio-brand-link">
            <img src={symbioLogo} alt="Symbio Hub logo" className="symbio-brand-logo" />
            <span>Symbio Hub</span>
          </Link>

          {showExpertDashboardMenu && (
            <button
              type="button"
              className={`symbio-menu-toggle ${isDashboardMenuOpen ? 'is-open' : ''}`.trim()}
              aria-label="Toggle dashboard submenu"
              aria-expanded={isDashboardMenuOpen}
              onClick={() => setIsDashboardMenuOpen(open => !open)}
            >
              <span className="symbio-menu-toggle-bar" />
              <span className="symbio-menu-toggle-bar" />
              <span className="symbio-menu-toggle-bar" />
            </button>
          )}

          {showExpertDashboardMenu && isDashboardMenuOpen && (
            <div className="symbio-dashboard-submenu">
              <a href="#dashboard" className="symbio-dashboard-submenu-link">Dashboard</a>
              <a href="#projects" className="symbio-dashboard-submenu-link">Projects</a>
              <a href="#milestones" className="symbio-dashboard-submenu-link">Milestones</a>
              <a href="#payments" className="symbio-dashboard-submenu-link">Payments</a>
              <a href="#reports" className="symbio-dashboard-submenu-link">Reports</a>
            </div>
          )}
        </div>
        <Link to="/jobs" className="symbio-link">Public jobs</Link>
        <Link to="/marketplace" className="symbio-link">Marketplace</Link>
        {session && session.role === 'SME' && (
          <Link to="/sme/dashboard" className="symbio-link">SME Dashboard</Link>
        )}
        {session && session.role === 'Expert' && (
          <Link to="/expert/dashboard" className="symbio-link">Expert Dashboard</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/control" className="symbio-link">Admin Dashboard</Link>
        )}
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
          <Link to="/admin/telemetry" className="symbio-link">Telemetry</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/compliance" className="symbio-link">Compliance Queue</Link>
        )}
        {session && session.role === 'Admin' && (
          <Link to="/admin/safety" className="symbio-link">Safety Overrides</Link>
        )}
        {session && (session.role === 'SME' || session.role === 'Expert') && <Link to="/onboarding" className="symbio-link">Trust onboarding</Link>}
        {session && <Link to="/settings" className="symbio-link">Settings</Link>}
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
