import React, { createContext, useContext, useState, useEffect } from 'react';

export type UserRole = 'SME' | 'Expert' | 'Admin';

interface UserSession {
  token: string;
  role: UserRole;
  email: string;
}

interface AuthContextType {
  session: UserSession | null;
  isLoading: boolean;
  login: (token: string, role: UserRole, email: string) => void;
  logout: () => void;
}

const TOKEN_STORAGE_KEY = 'symbio_auth_token';
const EMAIL_STORAGE_KEY = 'symbio_auth_email';
const LEGACY_ROLE_STORAGE_KEY = 'symbio_auth_role';

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split('.');
  if (parts.length < 2) {
    return null;
  }

  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const decoded = atob(base64);
    const payload = JSON.parse(decoded) as Record<string, unknown>;
    return payload;
  } catch {
    return null;
  }
}

export function decodeRoleFromToken(token: string): UserRole | null {
  const payload = decodeJwtPayload(token);
  if (!payload) {
    return null;
  }

  const roleFromShortClaim = payload.role;
  if (roleFromShortClaim === 'SME' || roleFromShortClaim === 'Expert' || roleFromShortClaim === 'Admin') {
    return roleFromShortClaim;
  }

  const roleFromDotnetClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  if (roleFromDotnetClaim === 'SME' || roleFromDotnetClaim === 'Expert' || roleFromDotnetClaim === 'Admin') {
    return roleFromDotnetClaim;
  }

  return null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [session, setSession] = useState<UserSession | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const storedToken = localStorage.getItem(TOKEN_STORAGE_KEY);
    const storedEmail = localStorage.getItem(EMAIL_STORAGE_KEY);

    if (storedToken && storedEmail) {
      const tokenRole = decodeRoleFromToken(storedToken);
      if (tokenRole) {
        setSession({ token: storedToken, role: tokenRole, email: storedEmail });
      }
    }

    // Cleanup legacy role cache to avoid role drift from local storage tampering.
    localStorage.removeItem(LEGACY_ROLE_STORAGE_KEY);
    setIsLoading(false);
  }, []);

  const login = (token: string, role: UserRole, email: string) => {
    const tokenRole = decodeRoleFromToken(token);
    if (!tokenRole || tokenRole !== role) {
      throw new Error('Session token role verification failed.');
    }

    localStorage.setItem(TOKEN_STORAGE_KEY, token);
    localStorage.setItem(EMAIL_STORAGE_KEY, email);
    localStorage.removeItem(LEGACY_ROLE_STORAGE_KEY);
    setSession({ token, role: tokenRole, email });
  };

  const logout = () => {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(EMAIL_STORAGE_KEY);
    localStorage.removeItem(LEGACY_ROLE_STORAGE_KEY);
    setSession(null);
  };

  return (
    <AuthContext.Provider value={{ session, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};
