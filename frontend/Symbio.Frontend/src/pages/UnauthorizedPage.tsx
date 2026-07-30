import React from 'react';
import { Link, useLocation } from 'react-router-dom';

export const UnauthorizedPage: React.FC = () => {
  const location = useLocation();
  const reason = (location.state as { reason?: string } | null)?.reason;

  return (
    <main className="symbio-page-main" style={{ textAlign: 'center' }}>
      <h1 className="symbio-page-title">Unauthorized</h1>
      <p>{reason ?? 'You do not have permission to access this page.'}</p>
      <p>
        <Link to="/" style={{ color: '#0072ce' }}>
          Return to public home
        </Link>
      </p>
    </main>
  );
};
