import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { LandingPage } from './pages/LandingPage';
import { PublicJobs } from './pages/PublicJobs';
import { LoginPage } from './pages/LoginPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { UnauthorizedPage } from './pages/UnauthorizedPage';
import { SmeDashboardPage } from './pages/SmeDashboardPage';
import { ExpertWorkbenchPage } from './pages/ExpertWorkbenchPage';
import { AdminControlPage } from './pages/AdminControlPage';
import { PublicRouteGuard } from './components/routing/PublicRouteGuard';
import { ProtectedRoute } from './components/routing/ProtectedRoute';

const App: React.FC = () => (
  <Routes>
    <Route path="/" element={<LandingPage />} />
    <Route path="/jobs" element={<PublicJobs />} />
    <Route path="/login" element={<PublicRouteGuard><LoginPage /></PublicRouteGuard>} />
    <Route path="/sme/dashboard" element={<ProtectedRoute allowedRoles={['SME']}><SmeDashboardPage /></ProtectedRoute>} />
    <Route path="/expert/workbench" element={<ProtectedRoute allowedRoles={['Expert']}><ExpertWorkbenchPage /></ProtectedRoute>} />
    <Route path="/admin/control" element={<ProtectedRoute allowedRoles={['Admin']}><AdminControlPage /></ProtectedRoute>} />
    <Route path="/unauthorized" element={<UnauthorizedPage />} />
    <Route path="/404" element={<NotFoundPage />} />
    <Route path="*" element={<Navigate to="/404" replace />} />
  </Routes>
);

export default App;
