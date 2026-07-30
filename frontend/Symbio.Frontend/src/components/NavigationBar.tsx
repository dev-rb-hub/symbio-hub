import React, { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { decodeRoleFromToken, useAuth } from '../context/AuthContext';
import symbioLogo from '../assets/images/Symbio-hub.png';

type DashboardMenuItem = {
  label: string;
  href: string;
};

const isDashboardMenuItemActive = (href: string, pathname: string, hash: string): boolean => {
  if (href.startsWith('#')) {
    return hash === href;
  }

  const [targetPath, targetHash = ''] = href.split('#');
  if (targetHash.length === 0) {
    return pathname === targetPath;
  }

  return pathname === targetPath && hash === `#${targetHash}`;
};

export const NavigationBar: React.FC = () => {
  const { session, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const role = session?.token ? decodeRoleFromToken(session.token) ?? session.role : session?.role;
  const isExpertDashboard = role === 'Expert' && location.pathname === '/expert/dashboard';
  const isSmeDashboard = role === 'SME' && location.pathname === '/sme/dashboard';
  const isAdminDashboard = role === 'Admin'
    && (
      location.pathname === '/admin/control'
      || location.pathname === '/admin/telemetry'
      || location.pathname === '/admin/compliance'
      || location.pathname === '/admin/agreements'
      || location.pathname === '/admin/safety'
    );

  const showDashboardMenu = isExpertDashboard || isSmeDashboard || isAdminDashboard;

  const dashboardMenuItems: DashboardMenuItem[] = isExpertDashboard
    ? [
        { label: 'Dashboard', href: '#dashboard' },
        { label: 'Projects', href: '#projects' },
        { label: 'Milestones', href: '#milestones' },
        { label: 'Payments', href: '#payments' },
        { label: 'Reports', href: '#reports' },
      ]
    : isSmeDashboard
      ? [
          { label: 'Dashboard', href: '#sme-dashboard' },
          { label: 'Runtime', href: '#sme-runtime' },
          { label: 'Summary', href: '#sme-summary' },
          { label: 'Invoices', href: '#sme-invoices' },
        ]
      : isAdminDashboard
        ? [
            { label: 'Dashboard', href: '/admin/control#admin-dashboard' },
            { label: 'Overview', href: '/admin/control#admin-overview' },
            { label: 'Telemetry', href: '/admin/telemetry#admin-telemetry' },
            { label: 'Compliance', href: '/admin/compliance#admin-compliance' },
            { label: 'Agreements', href: '/admin/agreements#admin-agreements' },
            { label: 'Safety', href: '/admin/safety#admin-safety' },
          ]
        : [];

  const [isDashboardMenuOpen, setIsDashboardMenuOpen] = useState(showDashboardMenu);

  useEffect(() => {
    if (!showDashboardMenu) {
      setIsDashboardMenuOpen(false);
      return;
    }

    setIsDashboardMenuOpen(true);
  }, [showDashboardMenu]);

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <nav className="symbio-nav">
      <div className="symbio-nav-links">
        <div className="symbio-brand-menu-wrap">
          <Link to="/" className="symbio-brand-link">
            <img src={symbioLogo} alt="Symbio Hub logo" className="symbio-brand-logo" />
            <span>Symbio Hub</span>
          </Link>

          {showDashboardMenu && (
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

          {showDashboardMenu && isDashboardMenuOpen && (
            <div className="symbio-dashboard-submenu">
              <div className="symbio-dashboard-submenu-title">Dashboard sections</div>
              {dashboardMenuItems.map(item => (
                <a
                  key={item.label}
                  href={item.href}
                  className={`symbio-dashboard-submenu-link ${isDashboardMenuItemActive(item.href, location.pathname, location.hash) ? 'symbio-dashboard-submenu-link--active' : ''}`.trim()}
                >
                  {item.label}
                </a>
              ))}
            </div>
          )}
        </div>
        <Link to="/jobs" className="symbio-link">Public jobs</Link>
        <Link to="/marketplace" className="symbio-link">Marketplace</Link>
        <Link to="/journey" className="symbio-link">Journey</Link>
        {session && <span className="symbio-role-spacer" aria-hidden="true" />}
        {role === 'SME' && (
          <Link to="/sme/dashboard" className="symbio-link symbio-link-role">SME Dashboard</Link>
        )}
        {role === 'Expert' && (
          <Link to="/expert/dashboard" className="symbio-link symbio-link-role">Expert Dashboard</Link>
        )}
        {role === 'Admin' && (
          <Link to="/admin/control" className="symbio-link symbio-link-role">Admin Dashboard</Link>
        )}
        {role === 'SME' && (
          <Link to="/talent/discovery" className="symbio-link symbio-link-role">Talent discovery</Link>
        )}
        {role === 'Expert' && (
          <Link to="/expert/workbench" className="symbio-link symbio-link-role">Workbench</Link>
        )}
        {role === 'Expert' && (
          <Link to="/escrow/onboarding" className="symbio-link symbio-link-role">Escrow onboarding</Link>
        )}
        {role === 'SME' && (
          <Link to="/project/new" className="symbio-link symbio-link-role">Post a Project</Link>
        )}
        {role === 'SME' && (
          <Link to="/billing/control-center" className="symbio-link symbio-link-role">Recurring Billing</Link>
        )}
        {session && (
          <Link to="/agreement" className="symbio-link symbio-link-role">Agreement</Link>
        )}
        {role === 'Admin' && (
          <Link to="/admin/telemetry" className="symbio-link symbio-link-role">Telemetry</Link>
        )}
        {role === 'Admin' && (
          <Link to="/admin/compliance" className="symbio-link symbio-link-role">Compliance Queue</Link>
        )}
        {role === 'Admin' && (
          <Link to="/admin/agreements" className="symbio-link symbio-link-role">Agreements</Link>
        )}
        {role === 'Admin' && (
          <Link to="/admin/safety" className="symbio-link symbio-link-role">Safety Overrides</Link>
        )}
        {(role === 'SME' || role === 'Expert') && <Link to="/onboarding" className="symbio-link symbio-link-role">Trust onboarding</Link>}
        {(role === 'SME' || role === 'Expert') && <Link to="/profile" className="symbio-link symbio-link-role">Profile</Link>}
        {role === 'Admin' && <Link to="/settings" className="symbio-link symbio-link-role">Settings</Link>}
      </div>
      <div className="symbio-nav-account">
        {session ? (
          <>
            <span
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                padding: '0.25rem 0.55rem',
                borderRadius: 999,
                background: role === 'Admin' ? '#1f3b57' : role === 'Expert' ? '#0b6e4f' : '#0f5ea8',
                color: '#fff',
                fontSize: '0.8rem',
                fontWeight: 700,
                letterSpacing: '0.02em',
                textTransform: 'uppercase'
              }}
            >
              Role: {role ?? 'Unknown'}
            </span>
            <span className="symbio-user-email">{session.email}</span>
            <button onClick={handleLogout} className="symbio-logout-button">
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
