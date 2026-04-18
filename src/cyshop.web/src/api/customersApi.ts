import type { CustomerAddress } from '../types/customer';
import { apiRequest } from './apiClient';

const BASE = '/api/customers';

export function fetchAddresses(): Promise<CustomerAddress[]> {
  return apiRequest<CustomerAddress[]>('GET', `${BASE}/addresses`, { requireAuth: true });
}
