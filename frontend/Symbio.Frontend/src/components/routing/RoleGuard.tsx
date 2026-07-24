import React from 'react';
import { UserRole } from '../../context/AuthContext';
import { ProtectedRoute } from './ProtectedRoute';

interface RoleGuardProps {
  children: React.ReactNode;
  role: UserRole;
}

export const RoleGuard: React.FC<RoleGuardProps> = ({ children, role }) => {
  return <ProtectedRoute allowedRoles={[role]}>{children}</ProtectedRoute>;
};