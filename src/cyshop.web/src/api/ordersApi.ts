import type { CreateOrderRequest, OrderSummary } from '../types/order';
import { apiRequest } from './apiClient';

const BASE = '/api/orders';

export function createOrder(request: CreateOrderRequest, idempotencyKey: string): Promise<void> {
  return apiRequest<void>('POST', BASE, {
    body: request,
    requireAuth: true,
    headers: { 'Idempotency-Key': idempotencyKey },
  });
}

export function fetchOrders(): Promise<OrderSummary[]> {
  return apiRequest<OrderSummary[]>('GET', BASE, { requireAuth: true });
}
