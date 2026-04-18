import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { userEvent } from '@testing-library/user-event';
import { AuthGuard } from '../auth/AuthGuard';
import { useAuthStatus } from '../auth/useAuthStatus';

// Shared mock state for react-oidc-context
const mockSigninRedirect = vi.fn();
const mockSignoutRedirect = vi.fn();

let mockAuth = {
  isAuthenticated: false,
  isLoading: false,
  activeNavigator: undefined as string | undefined,
  user: null as unknown,
  signinRedirect: mockSigninRedirect,
  signoutRedirect: mockSignoutRedirect,
};

vi.mock('react-oidc-context', () => ({
  useAuth: () => mockAuth,
}));

describe('AuthGuard', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockAuth = {
      isAuthenticated: false,
      isLoading: false,
      activeNavigator: undefined,
      user: null,
      signinRedirect: mockSigninRedirect,
      signoutRedirect: mockSignoutRedirect,
    };
  });

  it('renders children when authenticated', () => {
    mockAuth.isAuthenticated = true;
    render(
      <AuthGuard>
        <p>Protected content</p>
      </AuthGuard>,
    );
    expect(screen.getByText('Protected content')).toBeInTheDocument();
  });

  it('calls signinRedirect when not authenticated', () => {
    render(
      <AuthGuard>
        <p>Protected content</p>
      </AuthGuard>,
    );
    expect(mockSigninRedirect).toHaveBeenCalled();
    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  });

  it('shows default fallback while redirecting', () => {
    render(
      <AuthGuard>
        <p>Protected content</p>
      </AuthGuard>,
    );
    expect(screen.getByText(/redirecting to login/i)).toBeInTheDocument();
  });

  it('shows custom fallback when provided', () => {
    render(
      <AuthGuard fallback={<p>Please wait...</p>}>
        <p>Protected content</p>
      </AuthGuard>,
    );
    expect(screen.getByText('Please wait...')).toBeInTheDocument();
  });

  it('shows fallback while loading', () => {
    mockAuth.isLoading = true;
    render(
      <AuthGuard>
        <p>Protected content</p>
      </AuthGuard>,
    );
    expect(screen.getByText(/redirecting to login/i)).toBeInTheDocument();
    expect(screen.queryByText('Protected content')).not.toBeInTheDocument();
  });

  it('does not call signinRedirect while loading', () => {
    mockAuth.isLoading = true;
    render(
      <AuthGuard>
        <p>Protected content</p>
      </AuthGuard>,
    );
    expect(mockSigninRedirect).not.toHaveBeenCalled();
  });
});

// Test useAuthStatus via a small wrapper component
function AuthStatusDisplay() {
  const { isAuthenticated, user, login, logout } = useAuthStatus();
  return (
    <div>
      <span data-testid="auth">{String(isAuthenticated)}</span>
      <span data-testid="user">{user ? 'has-user' : 'no-user'}</span>
      <button onClick={login}>Login</button>
      <button onClick={logout}>Logout</button>
    </div>
  );
}

describe('useAuthStatus', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockAuth = {
      isAuthenticated: false,
      isLoading: false,
      activeNavigator: undefined,
      user: null,
      signinRedirect: mockSigninRedirect,
      signoutRedirect: mockSignoutRedirect,
    };
  });

  it('exposes isAuthenticated as false when not logged in', () => {
    render(<AuthStatusDisplay />);
    expect(screen.getByTestId('auth').textContent).toBe('false');
    expect(screen.getByTestId('user').textContent).toBe('no-user');
  });

  it('exposes isAuthenticated as true and user when logged in', () => {
    mockAuth.isAuthenticated = true;
    mockAuth.user = { profile: { sub: '123' } };
    render(<AuthStatusDisplay />);
    expect(screen.getByTestId('auth').textContent).toBe('true');
    expect(screen.getByTestId('user').textContent).toBe('has-user');
  });

  it('login calls signinRedirect', async () => {
    render(<AuthStatusDisplay />);
    await userEvent.click(screen.getByText('Login'));
    expect(mockSigninRedirect).toHaveBeenCalled();
  });

  it('logout calls signoutRedirect', async () => {
    mockAuth.isAuthenticated = true;
    render(<AuthStatusDisplay />);
    await userEvent.click(screen.getByText('Logout'));
    expect(mockSignoutRedirect).toHaveBeenCalled();
  });
});
