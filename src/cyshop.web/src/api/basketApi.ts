import type { CustomerBasket } from '../types/basket';
import { apiRequest } from './apiClient';

const BASE = '/api/basket';

export function fetchBasket(): Promise<CustomerBasket> {
  return apiRequest<CustomerBasket>('GET', BASE);
}

export function updateBasket(basket: CustomerBasket): Promise<CustomerBasket> {
  return apiRequest<CustomerBasket>('POST', BASE, {
    body: basket,
    requireAuth: true,
  });
}

export function deleteBasket(): Promise<void> {
  return apiRequest<void>('DELETE', BASE, { requireAuth: true });
}
