import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { decodeRoleFromToken, useAuth, UserRole } from '../../context/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: UserRole[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, allowedRoles }) => {
  const { session, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <div className="loading-spinner">Verifying identity secure rails...</div>;
  }

  if (!session) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  const tokenRole = decodeRoleFromToken(session.token);
  if (!tokenRole) {
    return <Navigate to="/login" state={{ from: location, reason: 'Your session token is invalid. Please sign in again.' }} replace />;
  }

  if (tokenRole !== session.role) {
    return <Navigate to="/unauthorized" state={{ reason: 'Your role claims could not be verified. Access has been blocked.' }} replace />;
  }

  if (allowedRoles && !allowedRoles.includes(tokenRole)) {
    return <Navigate to="/unauthorized" state={{ reason: 'Your account role does not have access to this area.' }} replace />;
  }

  return <>{children}</>;
};
