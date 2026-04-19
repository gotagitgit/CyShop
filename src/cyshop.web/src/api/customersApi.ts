import type { CustomerAddress, CustomerProfile } from '../types/customer';
import { apiRequest } from './apiClient';

const BASE = '/api/customers';

export interface CreateCustomerPayload {
  firstName: string;
  lastName: string;
  email: string;
  contactNumber: string;
}

export interface CreateAddressPayload {
  label: string;
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  isDefault: boolean;
}

// Profile API functions

export function fetchProfile(): Promise<CustomerProfile> {
  return apiRequest<CustomerProfile>('GET', `${BASE}/profile`, { requireAuth: true });
}

export function createProfile(data: CreateCustomerPayload): Promise<CustomerProfile> {
  return apiRequest<CustomerProfile>('POST', `${BASE}/profile`, { body: data, requireAuth: true });
}

export function updateProfile(data: CreateCustomerPayload): Promise<CustomerProfile> {
  return apiRequest<CustomerProfile>('PUT', `${BASE}/profile`, { body: data, requireAuth: true });
}

export function deleteProfile(): Promise<void> {
  return apiRequest<void>('DELETE', `${BASE}/profile`, { requireAuth: true });
}

// Address API functions

export function fetchAddresses(): Promise<CustomerAddress[]> {
  return apiRequest<CustomerAddress[]>('GET', `${BASE}/addresses`, { requireAuth: true });
}

export function createAddress(data: CreateAddressPayload): Promise<CustomerAddress> {
  return apiRequest<CustomerAddress>('POST', `${BASE}/addresses`, { body: data, requireAuth: true });
}

export function updateAddress(id: string, data: CreateAddressPayload): Promise<CustomerAddress> {
  return apiRequest<CustomerAddress>('PUT', `${BASE}/addresses/${id}`, { body: data, requireAuth: true });
}

export function deleteAddress(id: string): Promise<void> {
  return apiRequest<void>('DELETE', `${BASE}/addresses/${id}`, { requireAuth: true });
}
