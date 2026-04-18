import type { UserManager, User } from 'oidc-client-ts';

export interface ApiError {
  status: number;
  message: string;
  url: string;
}

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE';

let userManager: UserManager | null = null;
let cachedUser: User | null = null;

export function setUserManager(um: UserManager): void {
  userManager = um;

  // Keep cachedUser in sync with UserManager events
  um.events.addUserLoaded((user) => {
    cachedUser = user;
  });
  um.events.addUserUnloaded(() => {
    cachedUser = null;
  });

  // Initialize from storage
  um.getUser().then((user) => {
    cachedUser = user;
  });
}

export function getUserManager(): UserManager | null {
  return userManager;
}

async function getAccessToken(): Promise<string | null> {
  // First try the cached user (set by UserManager events)
  if (cachedUser?.access_token && !cachedUser.expired) {
    return cachedUser.access_token;
  }
  // Fall back to reading from storage
  if (!userManager) return null;
  const user = await userManager.getUser();
  if (user?.access_token) {
    cachedUser = user;
    return user.access_token;
  }
  return null;
}

async function renewToken(): Promise<string | null> {
  if (!userManager) return null;
  try {
    const user = await userManager.signinSilent();
    return user?.access_token ?? null;
  } catch {
    return null;
  }
}

export async function apiRequest<T>(
  method: HttpMethod,
  url: string,
  options?: { body?: unknown; requireAuth?: boolean }
): Promise<T> {
  const { body, requireAuth = false } = options ?? {};

  const headers: Record<string, string> = {};

  const token = await getAccessToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  } else if (requireAuth) {
    console.warn('[apiClient] requireAuth=true but no access token available');
  }

  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  const init: RequestInit = {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  };

  let response = await fetch(url, init);

  // Handle 401 with silent token renewal and single retry
  if (response.status === 401 && requireAuth) {
    const newToken = await renewToken();
    if (newToken) {
      headers['Authorization'] = `Bearer ${newToken}`;
      const retryInit: RequestInit = {
        method,
        headers,
        body: body !== undefined ? JSON.stringify(body) : undefined,
      };
      response = await fetch(url, retryInit);
    }
  }

  if (!response.ok) {
    let message: string;
    try {
      const errorBody = await response.text();
      message = errorBody || response.statusText;
    } catch {
      message = response.statusText;
    }

    const error: ApiError = {
      status: response.status,
      message,
      url,
    };
    throw error;
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}
