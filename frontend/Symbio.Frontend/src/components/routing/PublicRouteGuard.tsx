import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

interface PublicRouteGuardProps {
  children: React.ReactNode;
}

export const PublicRouteGuard: React.FC<PublicRouteGuardProps> = ({ children }) => {
  const { session, isLoading } = useAuth();

  if (isLoading) {
    return null;
  }

  if (session) {
    switch (session.role) {
      case 'SME':
        return <Navigate to="/sme/dashboard" replace />;
      case 'Expert':
        return <Navigate to="/expert/workbench" replace />;
      case 'Admin':
        return <Navigate to="/admin/control" replace />;
      default:
        return <Navigate to="/" replace />;
    }
  }

  return <>{children}</>;
};
