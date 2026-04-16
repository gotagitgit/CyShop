import type { UserManager } from 'oidc-client-ts';

export interface ApiError {
  status: number;
  message: string;
  url: string;
}

export type HttpMethod = 'GET' | 'POST' | 'DELETE';

let userManager: UserManager | null = null;

export function setUserManager(um: UserManager): void {
  userManager = um;
}

export function getUserManager(): UserManager | null {
  return userManager;
}

async function getAccessToken(): Promise<string | null> {
  if (!userManager) return null;
  const user = await userManager.getUser();
  return user?.access_token ?? null;
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

  if (requireAuth) {
    const token = await getAccessToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
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
