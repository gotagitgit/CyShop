import { useAuth } from 'react-oidc-context';
import type { User } from 'oidc-client-ts';

export interface AuthStatus {
  isAuthenticated: boolean;
  user: User | null;
  login: () => void;
  logout: () => void;
}

export function useAuthStatus(): AuthStatus {
  const auth = useAuth();

  return {
    isAuthenticated: auth.isAuthenticated,
    user: auth.user ?? null,
    login: () => void auth.signinRedirect(),
    logout: () => void auth.signoutRedirect(),
  };
}
