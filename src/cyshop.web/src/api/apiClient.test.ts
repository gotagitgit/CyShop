import { describe, it, expect, vi, beforeEach } from 'vitest';
import { apiRequest, setUserManager, getUserManager } from './apiClient';
import type { ApiError } from './apiClient';
import type { UserManager, User } from 'oidc-client-ts';

function createMockUserManager(overrides?: Partial<UserManager>): UserManager {
  return {
    getUser: vi.fn().mockResolvedValue(null),
    signinSilent: vi.fn().mockResolvedValue(null),
    events: {
      addUserLoaded: vi.fn(),
      addUserUnloaded: vi.fn(),
    },
    ...overrides,
  } as unknown as UserManager;
}

function mockFetchResponse(status: number, body?: unknown, statusText = 'OK'): void {
  const response = {
    ok: status >= 200 && status < 300,
    status,
    statusText,
    json: vi.fn().mockResolvedValue(body),
    text: vi.fn().mockResolvedValue(body !== undefined ? JSON.stringify(body) : ''),
  } as unknown as Response;
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(response);
}

describe('apiClient', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    setUserManager(createMockUserManager());
  });

  describe('setUserManager / getUserManager', () => {
    it('stores and retrieves the UserManager instance', () => {
      const um = createMockUserManager();
      setUserManager(um);
      expect(getUserManager()).toBe(um);
    });
  });

  describe('apiRequest - unauthenticated', () => {
    it('makes a GET request without Authorization header', async () => {
      const data = [{ id: '1', name: 'Item' }];
      mockFetchResponse(200, data);

      const result = await apiRequest('GET', '/api/catalog');

      expect(fetch).toHaveBeenCalledWith('/api/catalog', {
        method: 'GET',
        headers: {},
        body: undefined,
      });
      expect(result).toEqual(data);
    });

    it('sends JSON body for POST requests', async () => {
      const payload = { buyerId: 'user1', items: [] };
      mockFetchResponse(200, payload);

      await apiRequest('POST', '/api/basket', { body: payload });

      const callArgs = vi.mocked(fetch).mock.calls[0];
      expect(callArgs[1]?.method).toBe('POST');
      expect((callArgs[1]?.headers as Record<string, string>)['Content-Type']).toBe('application/json');
      expect(callArgs[1]?.body).toBe(JSON.stringify(payload));
    });
  });

  describe('apiRequest - authenticated', () => {
    it('attaches Bearer token when requireAuth is true', async () => {
      const um = createMockUserManager({
        getUser: vi.fn().mockResolvedValue({ access_token: 'test-token-123' } as User),
      });
      setUserManager(um);
      mockFetchResponse(200, { ok: true });

      await apiRequest('GET', '/api/basket', { requireAuth: true });

      const callArgs = vi.mocked(fetch).mock.calls[0];
      expect((callArgs[1]?.headers as Record<string, string>)['Authorization']).toBe('Bearer test-token-123');
    });

    it('attaches token even when requireAuth is false if user is available', async () => {
      const um = createMockUserManager({
        getUser: vi.fn().mockResolvedValue({ access_token: 'opportunistic-token' } as User),
      });
      setUserManager(um);
      mockFetchResponse(200, []);

      await apiRequest('GET', '/api/catalog');

      const callArgs = vi.mocked(fetch).mock.calls[0];
      expect((callArgs[1]?.headers as Record<string, string>)['Authorization']).toBe('Bearer opportunistic-token');
    });
  });

  describe('apiRequest - 401 retry', () => {
    it('retries with renewed token on 401', async () => {
      const um = createMockUserManager({
        getUser: vi.fn().mockResolvedValue({ access_token: 'expired-token' } as User),
        signinSilent: vi.fn().mockResolvedValue({ access_token: 'fresh-token' } as User),
      });
      setUserManager(um);

      const failResponse = {
        ok: false,
        status: 401,
        statusText: 'Unauthorized',
        text: vi.fn().mockResolvedValue('Unauthorized'),
      } as unknown as Response;

      const successResponse = {
        ok: true,
        status: 200,
        statusText: 'OK',
        json: vi.fn().mockResolvedValue({ data: 'ok' }),
      } as unknown as Response;

      vi.spyOn(globalThis, 'fetch')
        .mockResolvedValueOnce(failResponse)
        .mockResolvedValueOnce(successResponse);

      const result = await apiRequest('GET', '/api/basket', { requireAuth: true });

      expect(fetch).toHaveBeenCalledTimes(2);
      const retryHeaders = vi.mocked(fetch).mock.calls[1][1]?.headers as Record<string, string>;
      expect(retryHeaders['Authorization']).toBe('Bearer fresh-token');
      expect(result).toEqual({ data: 'ok' });
    });

    it('throws ApiError when renewal fails on 401', async () => {
      const um = createMockUserManager({
        getUser: vi.fn().mockResolvedValue({ access_token: 'expired-token' } as User),
        signinSilent: vi.fn().mockResolvedValue(null),
      });
      setUserManager(um);

      const failResponse = {
        ok: false,
        status: 401,
        statusText: 'Unauthorized',
        text: vi.fn().mockResolvedValue('Unauthorized'),
      } as unknown as Response;

      vi.spyOn(globalThis, 'fetch').mockResolvedValue(failResponse);

      try {
        await apiRequest('GET', '/api/basket', { requireAuth: true });
        expect.fail('Should have thrown');
      } catch (err) {
        const apiErr = err as ApiError;
        expect(apiErr.status).toBe(401);
        expect(apiErr.url).toBe('/api/basket');
      }
    });
  });

  describe('apiRequest - error handling', () => {
    it('throws ApiError with status and message on non-ok response', async () => {
      const errorResponse = {
        ok: false,
        status: 404,
        statusText: 'Not Found',
        text: vi.fn().mockResolvedValue('Item not found'),
      } as unknown as Response;

      vi.spyOn(globalThis, 'fetch').mockResolvedValue(errorResponse);

      try {
        await apiRequest('GET', '/api/catalog/missing-id');
        expect.fail('Should have thrown');
      } catch (err) {
        const apiErr = err as ApiError;
        expect(apiErr.status).toBe(404);
        expect(apiErr.message).toBe('Item not found');
        expect(apiErr.url).toBe('/api/catalog/missing-id');
      }
    });

    it('handles 204 No Content', async () => {
      const noContentResponse = {
        ok: true,
        status: 204,
        statusText: 'No Content',
      } as unknown as Response;

      vi.spyOn(globalThis, 'fetch').mockResolvedValue(noContentResponse);

      const result = await apiRequest('DELETE', '/api/basket', { requireAuth: true });
      expect(result).toBeUndefined();
    });
  });
});
