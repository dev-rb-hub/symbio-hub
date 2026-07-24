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
import { TrustOnboardingPage } from './pages/TrustOnboardingPage';
import { UserProfilePage } from './pages/UserProfilePage';
import { ProjectWizardPage } from './pages/ProjectWizardPage';
import { MarketplacePage } from './pages/MarketplacePage';
import { TalentDiscoveryPage } from './pages/TalentDiscoveryPage';
import { EscrowOnboardingPage } from './pages/EscrowOnboardingPage';
import { PublicRouteGuard } from './components/routing/PublicRouteGuard';
import { ProtectedRoute } from './components/routing/ProtectedRoute';
import { RoleGuard } from './components/routing/RoleGuard';
import { NavigationBar } from './components/NavigationBar';

const App: React.FC = () => (
  <>
    <NavigationBar />
    <Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/jobs" element={<PublicJobs />} />
      <Route path="/marketplace" element={<MarketplacePage />} />
      <Route path="/talent/discovery" element={<ProtectedRoute allowedRoles={['SME']}><TalentDiscoveryPage /></ProtectedRoute>} />
      <Route path="/project/new" element={<ProtectedRoute allowedRoles={['SME']}><ProjectWizardPage /></ProtectedRoute>} />
      <Route path="/escrow/onboarding" element={<ProtectedRoute allowedRoles={['Expert']}><EscrowOnboardingPage /></ProtectedRoute>} />
      <Route path="/login" element={<PublicRouteGuard><LoginPage /></PublicRouteGuard>} />
      <Route path="/onboarding" element={<ProtectedRoute allowedRoles={['SME', 'Expert']}><TrustOnboardingPage /></ProtectedRoute>} />
      <Route path="/profile" element={<ProtectedRoute allowedRoles={['SME', 'Expert', 'Admin']}><UserProfilePage /></ProtectedRoute>} />
      <Route path="/sme/dashboard" element={<RoleGuard role="SME"><SmeDashboardPage /></RoleGuard>} />
      <Route path="/expert/workbench" element={<RoleGuard role="Expert"><ExpertWorkbenchPage /></RoleGuard>} />
      <Route path="/admin/control" element={<RoleGuard role="Admin"><AdminControlPage /></RoleGuard>} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="/404" element={<NotFoundPage />} />
      <Route path="*" element={<Navigate to="/404" replace />} />
    </Routes>
  </>
);

export default App;
