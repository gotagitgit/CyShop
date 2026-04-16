import type { CatalogItemDto } from '../types/catalog';
import { apiRequest } from './apiClient';

const BASE = '/api/catalog';

export function fetchCatalogItems(): Promise<CatalogItemDto[]> {
  return apiRequest<CatalogItemDto[]>('GET', BASE);
}

export function fetchCatalogItemById(id: string): Promise<CatalogItemDto> {
  return apiRequest<CatalogItemDto>('GET', `${BASE}/${id}`);
}

export function fetchCatalogItemsByType(typeId: string): Promise<CatalogItemDto[]> {
  return apiRequest<CatalogItemDto[]>('GET', `${BASE}/type/${typeId}`);
}

export function fetchCatalogItemsByBrand(brandId: string): Promise<CatalogItemDto[]> {
  return apiRequest<CatalogItemDto[]>('GET', `${BASE}/brand/${brandId}`);
}

export function getCatalogImageUrl(id: string): string {
  return `${BASE}/${id}/pic`;
}
