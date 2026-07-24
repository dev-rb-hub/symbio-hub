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

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [session, setSession] = useState<UserSession | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const storedToken = localStorage.getItem('symbio_auth_token');
    const storedRole = localStorage.getItem('symbio_auth_role') as UserRole;
    const storedEmail = localStorage.getItem('symbio_auth_email');

    if (storedToken && storedRole && storedEmail) {
      setSession({ token: storedToken, role: storedRole, email: storedEmail });
    }
    setIsLoading(false);
  }, []);

  const login = (token: string, role: UserRole, email: string) => {
    localStorage.setItem('symbio_auth_token', token);
    localStorage.setItem('symbio_auth_role', role);
    localStorage.setItem('symbio_auth_email', email);
    setSession({ token, role, email });
  };

  const logout = () => {
    localStorage.removeItem('symbio_auth_token');
    localStorage.removeItem('symbio_auth_role');
    localStorage.removeItem('symbio_auth_email');
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
