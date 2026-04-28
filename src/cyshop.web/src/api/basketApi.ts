import type { CustomerBasket, BasketItem } from '../types/basket';
import { apiRequest } from './apiClient';

const BASE = '/api/basket';

export function fetchBasket(): Promise<CustomerBasket> {
  return apiRequest<CustomerBasket>('GET', BASE, { requireAuth: true });
}

export function updateBasket(basket: { items: BasketItem[] }): Promise<CustomerBasket> {
  return apiRequest<CustomerBasket>('POST', BASE, {
    body: { items: basket.items },
    requireAuth: true,
  });
}

export function deleteBasket(): Promise<void> {
  return apiRequest<void>('DELETE', BASE, { requireAuth: true });
}
