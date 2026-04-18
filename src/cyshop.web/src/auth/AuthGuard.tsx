import { useEffect, type ReactNode } from 'react';
import { useAuth } from 'react-oidc-context';

interface AuthGuardProps {
  children: ReactNode;
  fallback?: ReactNode;
}

export function AuthGuard({ children, fallback }: AuthGuardProps) {
  const auth = useAuth();

  useEffect(() => {
    if (!auth.isAuthenticated && !auth.isLoading && !auth.activeNavigator) {
      void auth.signinRedirect();
    }
  }, [auth.isAuthenticated, auth.isLoading, auth.activeNavigator, auth]);

  if (auth.isLoading || auth.activeNavigator) {
    return <>{fallback ?? <p>Redirecting to login…</p>}</>;
  }

  if (!auth.isAuthenticated) {
    return <>{fallback ?? <p>Redirecting to login…</p>}</>;
  }

  return <>{children}</>;
}
